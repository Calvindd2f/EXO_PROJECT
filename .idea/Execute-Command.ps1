function Execute-Command
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] 
        $CmdletName,

        [Parameter(Mandatory)]
        $Parameters,

        $ParameterBasedRoutingHintDelegate,

        [string] 
        $ParameterBasedRoutingHintParameterValue,

        [boolean]
        $ShowConfirmPrompt = $false,

        $PSCmdletObject,

        [boolean]
        $UseParameterBasedRouting = $false,

        [string]
        $CmdletRequestId
    )
    process
    {
        $script:DefaultPageSize = 1000
        if ($ConnectionContextObjectId -ne $null)
        {
            $ConnectionContext = [Microsoft.Exchange.Management.ExoPowershellSnapin.ConnectionContextFactory]::GetCurrentConnectionContext($ConnectionContextObjectId)
            if (($ConnectionContext -ne $null) -and ($ConnectionContext.PageSize -ge 1) -and ($ConnectionContext.PageSize -le 1000))
            {
                $script:DefaultPageSize = $ConnectionContext.PageSize
            }
        }

        $resultSize = $script:DefaultPageSize
        $nextPageSize = $script:DefaultPageSize
        $anotherPagedQuery = $true

        # Set the generic headers
        $Headers = @{
            'Accept'         = 'application/json'
            'Accept-Charset' = 'UTF-8'
            'Content-Type'   = 'application/json'
        }


        # Set cmdlet name in request header
        $Headers['X-CmdletName'] = $CmdletName



        # Set a client requestid to uniquely identify this cmdlet execution on the server.
        # This id will be the same for all retries, and all requests in a batch so it is set here itself.
        $Headers['client-request-id'] = $CmdletRequestId

        $WarningActionValue = ''
        if ($Parameters.ContainsKey('WarningAction'))
        {
            $WarningActionValue = $Parameters['WarningAction']
            if ($warningActionValue -eq 'Ignore')
            {
                # Treating Ignore WarningAction as SilentlyContinue to maintain RPS parity
                # This is for PS understanding to treat Ignore as SilentlyContinue
                $WarningPreference = 'SilentlyContinue'
                # This is for sending to server side
                $WarningActionValue = 'SilentlyContinue'
            }
        }
        $Headers['WarningAction'] = $WarningActionValue


        $null = $Parameters.Remove('UseCustomRouting');


        # Construct the request body
        $params = @{}
        $cmdletClass = Get-Command $CmdletName
        
        $params = Transform-Parameters -Parameters $Parameters -cmdletClass $cmdletClass

        if ($ShowConfirmPrompt)
        {
            if ($params.ContainsKey('Confirm') -eq $false)
            {
                $params.Add('Confirm', $true)
            }
    
            $UseBatching = $false
        }

        $cmdletInput = @{}
        $cmdletInput['CmdletName'] = $CmdletName
        $cmdletInput['Parameters'] = $params
        $BodyObj = @{}
        $BodyObj['CmdletInput'] = $cmdletInput

                  
        if ($UseBatching)
        {
            AddTo-BatchRequest -BodyObj $BodyObj -ParameterBasedRoutingHintDelegate $ParameterBasedRoutingHintDelegate -ParameterBasedRoutingHintParameterValue $ParameterBasedRoutingHintParameterValue -UseParameterBasedRouting $UseParameterBasedRouting -CmdletRequestId $CmdletRequestId

            if ($BatchBodyObj['requests'].Count -eq 10) 
            {
                $BatchResponse = Execute-BatchRequest -CmdletRequestId $CmdletRequestId
                Deserialize-BatchRequest -BatchResponse $BatchResponse -CmdletRequestId $CmdletRequestId
                $BatchBodyObj['requests'] = New-Object System.Collections.ArrayList;
                $endTime = Get-Date;
                Log-Column -ColumnName EndTime -CmdletId $CmdletRequestId -Value $endTime
            }
            return
        }


        $Body = ConvertTo-Json $BodyObj -Depth 10 -Compress
        $Body = ([System.Text.Encoding]::UTF8.GetBytes($Body));

        $resultSize = Get-ResultSize $Parameters
        if ($resultSize -eq -1)
        {
            return
        }
        
        if ($resultSize -ne 'Unlimited')
        {
            $nextPageSize = [Math]::min($resultSize, $script:DefaultPageSize)
        }
        $nextPageUri = ''

        while ($anotherPagedQuery)
        {
            # Update the next page size in the header
            $Headers['Prefer'] = 'odata.maxpagesize={0}' -f $nextPageSize
            $waitTime = 0 # In milliseconds

            $isRetryHappening = $true
            $isQuerySuccessful = $false
            $claims = [string]::Empty
            for ($retryCount = 0; $retryCount -le $script:DefaultMaxRetryTimes -and $isRetryHappening; $retryCount++)
            {
                try
                {

                    if ($ConnectionContextObjectId -ne $null)
                    {
                        $ConnectionContext = [Microsoft.Exchange.Management.ExoPowershellSnapin.ConnectionContextFactory]::GetCurrentConnectionContext($ConnectionContextObjectId);
                        $AuthInfo = $ConnectionContext.GetAuthHeader($claims, $cmdletRequestId);
                        $claims = [string]::Empty
                    }
                    else
                    {
                        # ConnectionContextObjectId is not present, so request is from an older version of client. Hence use AuthHeaderUtils.
                        $AuthInfo = Get-AuthInfoUsingTokenProvider $TokenProviderObjectId
                    }

                    
                    # Initialize per request logging
                    $startTime = Get-Date
                    Init-Log -CmdletId $CmdletRequestId
                    Log-Column -ColumnName StartTime -CmdletId $CmdletRequestId -Value $startTime
                    Log-Column -ColumnName CmdletName -CmdletId $CmdletRequestId -Value $CmdletName
                    Log-Column -ColumnName CmdletParameters -CmdletId $CmdletRequestId -Value $Parameters
                    $Uri = if ([string]::IsNullOrWhitespace($nextPageUri)) { $AuthInfo.BaseUri + '/' + $AuthInfo.TenantId + '/InvokeCommand' } else { $nextPageUri };

                    Populate-Headers -ParameterBasedRoutingHintDelegate $ParameterBasedRoutingHintDelegate -ParameterBasedRoutingHintParameterValue $ParameterBasedRoutingHintParameterValue -UseParameterBasedRouting $UseParameterBasedRouting -AuthInfo $AuthInfo -Headers $Headers

                    $existingProgressPreference = $global:ProgressPreference
                    $global:ProgressPreference = 'SilentlyContinue'

                    if ($ShowConfirmPrompt)
                    {
                        Write-Verbose('Validation call to the server:')
                    }

                    $timeout = Get-HttpTimeout $Headers
                    $Result = Invoke-WebRequest -UseBasicParsing -TimeoutSec $timeout -Method 'POST' -Headers $Headers -Body $Body -Uri $Uri 
                    $confirmationMessage = ''
                    if ($Result -ne $null -and $Result.Content -ne $null)
                    {
                        $ResultObject = (ConvertFrom-Json $Result.Content)
                        $confirmationMessage = $ResultObject.'@adminapi.confirmationmessage';
                    }

                    #ShouldProcess call should be made before checking the value of $ShowConfirmPrompt for supporting WhatIf behavior
                    if ($PSCmdletObject -ne $null -and $PSCmdletObject.ShouldProcess($confirmationMessage, $null, $null) -and $ShowConfirmPrompt)
                    {
                        $params['Confirm'] = $false

                        $cmdletInput = @{}
                        $cmdletInput['CmdletName'] = $CmdletName
                        $cmdletInput['Parameters'] = $params
                        $BodyObj = @{}
                        $BodyObj['CmdletInput'] = $cmdletInput

                        $Body = ConvertTo-Json $BodyObj -Depth 10 -Compress
                        $Body = ([System.Text.Encoding]::UTF8.GetBytes($Body));

                        Write-Verbose('Execution call to the server:')
                        $timeout = Get-HttpTimeout $Headers
                        $Result = Invoke-WebRequest -UseBasicParsing -TimeoutSec $timeout -Method 'POST' -Headers $Headers -Body $Body -Uri $Uri 
                    }
                    $global:ProgressPreference = $existingProgressPreference
                    $isQuerySuccessful = $true
                    $isRetryHappening = $false

                    $endTime = Get-Date;
                    Log-Column -ColumnName EndTime -CmdletId $CmdletRequestId -Value $endTime
                    foreach ($id in $cmdletIDList)
                    {
                        Commit-Log -CmdletId $id
                    }
            
                }
                catch
                {
                    $global:ProgressPreference = $existingProgressPreference
                    $claims = Get-ClaimsFromExceptionDetails -ErrorObject $_
                    $isRetryable = CheckRetryAndHandleWaitTime -retryCount $retryCount -ErrorObject $_ -CmdletRequestId $CmdletRequestId -IsFromBatchingRequest $false
                    if ($isRetryable -eq $false)
                    {
                        $global:EXO_LastExecutionStatus = $false;
                        return
                    }
                    else
                    {
                        # Log error for per request logging if it is retriable error, non retriable error gets logged in CheckRetryAndHandleWaitTime
                        Log-Column -ColumnName GenericError -CmdletId $CmdletRequestId -Value $_
                        $endTime = Get-Date;
                        Log-Column -ColumnName EndTime -CmdletId $CmdletRequestId -Value $endTime
                        foreach ($id in $cmdletIDList)
                        {
                            Commit-Log -CmdletId $id
                        }
                
                    }
                }
            }

            # Result Handling With Pagination Response
            if ($Result -ne $null -and $Result.Content -ne $null)
            {
                $ResultObject = (ConvertFrom-Json $Result.Content)

                # Print the Result
                PrintResultAndCheckForNextPage -ResultObject $ResultObject -VerifyNextPageLinkCheckRequired $true -anotherPagedQuery ([ref]$anotherPagedQuery) -resultSize $resultSize -nextPageSize $nextPageSize -isFromBatchingRequest:$false
                if ($anotherPagedQuery -eq $true)
                {
                    $nextPageUri = $ResultObject.'@odata.nextLink'
                    if ($resultSize -ne 'Unlimited')
                    {
                        $resultSize -= $nextPageSize
                        $nextPageSize = [Math]::min($resultSize, $script:DefaultPageSize)       
                    }
                }

            }
            else
            {
                $anotherPagedQuery = $false
            }
        }
    }
}










































































function Get-ErrorCategoryInfo
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [AllowNull()]
        $ErrorObject
    )
    process
    {
        $errorCategoryInfo = [PSCustomObject]@{
            Activity   = $CmdletName
            Category   = [Management.Automation.ErrorCategory]::NotSpecified
            Reason     = 'Exception'
            TargetName = $null
            TargetType = $null
        }

        if ($ErrorObject -ne $null -and $ErrorObject.ErrorDetails -ne $null -and $ErrorObject.ErrorDetails.Message -ne $null)
        {
            $errorDetailsPSObject = ConvertFrom-Json $ErrorObject.ErrorDetails.Message
            if ($errorDetailsPSObject.error -ne $null -and $errorDetailsPSObject.error.details -ne $null -and $errorDetailsPSObject.error.details.length -gt 0)
            {
                if (![string]::IsNullOrEmpty($errorDetailsPSObject.error.details[0].target))
                {
                    $errorCategoryInfo.TargetName = $errorDetailsPSObject.error.details[0].target
                }

                $errorCategory = $errorDetailsPSObject.error.details[0].code
                if (($errorCategory -as [int]) -ne $null)
                {
                    $errorCategory = $errorCategory -as [int]
                }
           
                
                if ($errorCategory -ne $null -and [Enum]::IsDefined([Management.Automation.ErrorCategory], $errorCategory))
                {
                    $errorCategoryInfo.Category = [Management.Automation.ErrorCategory]$errorCategory
                }
            }
        }

        if ($ErrorObject -ne $null -and $ErrorObject.Exception -ne $null -and $ErrorObject.Exception.Response -ne $null -and $ErrorObject.Exception.Response.Headers -ne $null)
        {
            $responseHeaders = $ErrorObject.Exception.Response.Headers
            if ($UseBatching)
            {
                $errorCategoryInfo.Activity = $responseHeaders['Cmdlet-Name']
            }

            if ($responseHeaders['x-ExceptionType'] -ne $null)
            {
                $errorCategoryInfo.Reason = $responseHeaders['x-ExceptionType']
            }

            if ($responseHeaders['x-ExceptionTargetType'] -ne $null)
            {
                $errorCategoryInfo.TargetType = $responseHeaders['x-ExceptionTargetType']
            }
        }
        
        return $errorCategoryInfo
    }
}
