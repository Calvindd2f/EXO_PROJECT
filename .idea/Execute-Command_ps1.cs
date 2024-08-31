using System.Security.AccessControl;
using System;
using System.Management.Automation;
using System.Collections;

namespace .
{

    [Cmdlet(VerbsLifecycle.Invoke, "ExecuteCommand")]
    [OutputType(typeof(object))]
    public class ExecuteCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateNotNullOrEmpty]
        public string CmdletName { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        [ValidateNotNull]
        public Hashtable Parameters { get; set; }

        [Parameter]
        public string ParameterBasedRoutingHintDelegate { get; set; }

        [Parameter]
        public string ParameterBasedRoutingHintParameterValue { get; set; }

        [Parameter]
        public SwitchParameter ShowConfirmPrompt { get; set; }

        [Parameter]
        public PSCmdlet PSCmdletObject { get; set; }

        [Parameter]
        public SwitchParameter UseParameterBasedRouting { get; set; }

        [Parameter]
        [ValidateNotNullOrEmpty]
        public string CmdletRequestId { get; set; }

        //ENDOF: Parameters

        // Begin:
        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            // Add any initialization logic here
            private static int DefaultPageSize;

            if (ConnectionContextObjectId != null)
            {
                var connectionContext = Microsoft.Exchange.Management.ExoPowershellSnapin.ConnectionContextFactory.GetCurrentConnectionContext(ConnectionContextObjectId);
                
                if (connectionContext != null && connectionContext.PageSize >= 1 && connectionContext.PageSize <= 1000)
                {
                    DefaultPageSize = connectionContext.PageSize;
                }
            }
        }

        protected override void ProcessRecord()
        {
            if (ConnectionContextObjectId != null)
            {
                var connectionContext = ConnectionContextFactory.GetCurrentConnectionContext(ConnectionContextObjectId);
                if (connectionContext != null && connectionContext.PageSize >= 1 && connectionContext.PageSize <= 1000)
                {
                    DefaultPageSize = connectionContext.PageSize;
                }
            }

            int resultSize = DefaultPageSize;
            int nextPageSize = DefaultPageSize;
            bool anotherPagedQuery = true;

            // Set the generic headers
            var headers = new Hashtable
            {
                { "Accept", "application/json" },
                { "Accept-Charset", "UTF-8" },
                { "Content-Type", "application/json" }
            };

            // Set cmdlet name in request header
            headers["X-CmdletName"] = CmdletName;

            // Set a client requestid to uniquely identify this cmdlet execution on the server.
            headers["client-request-id"] = CmdletRequestId;

            string warningActionValue = "";
            if (Parameters.ContainsKey("WarningAction"))
            {
                warningActionValue = Parameters["WarningAction"].ToString();
                if (string.Equals(warningActionValue, "Ignore", StringComparison.OrdinalIgnoreCase))
                {
                    // Treating Ignore WarningAction as SilentlyContinue to maintain RPS parity
                    // This is for PS understanding to treat Ignore as SilentlyContinue
                    WarningPreference = ActionPreference.SilentlyContinue;
                    // This is for sending to server side
                    warningActionValue = "SilentlyContinue";
                }
            }
            headers["WarningAction"] = warningActionValue;

            Parameters.Remove("UseCustomRouting");




            // # Construct the request body
            var parameters = new Hashtable();
            var cmdletClass = GetCommand(CmdletName);

            parameters = TransformParameters(Parameters, cmdletClass);

            if (ShowConfirmPrompt)
            {
                if (!parameters.ContainsKey("Confirm"))
                {
                    parameters.Add("Confirm", true);
                }

                UseBatching = false;
            }

            var cmdletInput = new Hashtable
                {
                    { "CmdletName", CmdletName },
                    { "Parameters", parameters }
                };

            var bodyObj = new Hashtable
                {
                    { "CmdletInput", cmdletInput }
                };

            if (UseBatching)
            {
                AddToBatchRequest(bodyObj, ParameterBasedRoutingHintDelegate, ParameterBasedRoutingHintParameterValue, UseParameterBasedRouting, CmdletRequestId);

                if (((ArrayList)BatchBodyObj["requests"]).Count == 10)
                {
                    var batchResponse = ExecuteBatchRequest(CmdletRequestId);
                    DeserializeBatchRequest(batchResponse, CmdletRequestId);
                    BatchBodyObj["requests"] = new ArrayList();
                    var endTime = DateTime.Now;
                    LogColumn("EndTime", CmdletRequestId, endTime);
                }
                return;
            }

            


            var Body = ConvertToJson(BodyObj,10,Compress)
            var Body = System.Text.Encoding.UTF8.GetBytes(BodyObj.ToString());

            resultSize = Get-ResultSize(Parameters)    

            if (resultSize === -1)
            {
                return
            }

            if (resultSize != 'Unlimited')
            {
                nextPageSize = Math.Min(resultSize, DefaultPageSize)
            }
            
            var nextPageUri = ''




        while (anotherPagedQuery)
        {
            // Update the next page size in the header
            Headers['Prefer'] = 'odata.maxpagesize={nextPageSize}'
            waitTime = 0    // In milliseconds


            isRetryHappening = true;
            isQuerySuccessful = false;
            claims = string.Empty;











        }












}
    }

}

/**************************************************************

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






****************************************************************/

/****************************************************************

        This is just scratched out for a begin/process/end block simulation.

        protected override void ProcessRecord()
        {
            // Add your main cmdlet logic here
            // For example:
            // var result = ExecuteCommandLogic();
            // WriteObject(result);
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            // Add any cleanup logic here
        }

        // Add any private methods or helper functions here
    }
}*/