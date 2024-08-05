#region interesting
    # EHGraph
    enum EHException
    {
        UserAccessDenied = 404;
        ResourceNotFound = 403;
        Throttling = 429;
        ServiceUnavailable = 503;
    }
    # API
    $success = $false
    $WaitTime = 30
    $RetryCount = 0    
    $RetryCodes = @(503, 504, 520, 521, 522, 524)
    $FailCodes = @(400, 404)
    $ThrottlingCode = @(429)
    while ($RetryCount -lt $retry -and $success -eq $false) {}
    # Exponential backoff explicitly for `$ThrottlingCode | 429
    $retryCount++
    $delay = [math]::Pow(2, $retryCount)
    ## Maybe log something
    $delay
#endregion





function Get-Token
{
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('https://graph.microsoft.com/.default', 'https://outlook.office365.com/.default')]
        [string]$Scope,

        [Parameter(Mandatory = $false)]
        [string]$AppId,

        [Parameter(Mandatory = $false)]
        [string]$AppSecret,

        [Parameter(Mandatory = $false)]
        [string]$TenantId
    )
    begin
    {
        $body = @{
            Grant_Type    = 'client_credentials'
            Client_Id     = $env:AppId
            Client_Secret = $env:AppSecret
            Scope         = $Scope
        }
    }
    process
    {
        $response = Invoke-RestMethod -Uri "https://login.microsoftonline.com/$env:TenantId/oauth2/v2.0/token" -Method Post -Body $body
    }
    end
    {
        $response.access_token
    }
}

function Example
{
    <# HELPDOC #>
    Param(

    )
    Begin
    {

    }
    Process
    {

    }
    End
    {

    }
}

Function ExecuteQuery($url, $token)
{
    try
    {
        $request = [System.Net.HttpWebRequest]::Create($url)
    
        $request.Method = 'GET';
        $request.ContentType = 'application/json;odata.metadata=minimal';
        $request.Headers['Authorization'] = "Bearer $token";
    
        try
        {
            $response = $request.GetResponse();
        }
        catch { }
        #Neither tenant is B2C or tenant doesn't have premium license
        $reader = New-Object System.IO.StreamReader $response.GetResponseStream();
        $jsonResult = $reader.ReadToEnd();
        $response.Dispose();
    
        return $jsonResult;
    }
    catch
    {
        Write-Host "$url"
        Write-Host $($_.Exception.Message)
        return $null;
    }
    
}
Function ExecuteQuery($url, $token)
{
    try
    {
        $request = [System.Net.HttpWebRequest]::Create($url)
    
        $request.Method = 'GET';
        $request.ContentType = 'application/json;odata.metadata=minimal';
        $request.Headers['Authorization'] = "Bearer $token";
        $request.Headers['ConsistencyLevel'] = 'eventual';
    
        $response = $request.GetResponse();
        $reader = New-Object System.IO.StreamReader $response.GetResponseStream();
        $jsonResult = $reader.ReadToEnd();
        $response.Dispose();
    
        return $(ConvertFrom-Json $jsonResult).value
        
    }
    catch
    {
        Write-Host 'Unable to execute command. Invalid input.'
        return $null
    }
}

Function GetDebugAPI($email, $query, $companyid, $userObjectToAction, $retry = 5)
{
    if ($Global:MxSDebug) { Write-Host $("`r`n`r`n==FUNC==CALLED===== -->{0}<--`r`n====ARGs=START======`r`n{1}`r`n====ARGs=END========`r`n" -f $MyInvocation.MyCommand, $(ConvertTo-Json $PSBoundParameters)) -ForegroundColor Yellow }
    if ($Global:MxSDebug) { Write-Host "========================= GetContact params email, query, companyid,: $email, $query, $companyid," }
    if ($query -eq 'Name')
    {
        $querystring = '?conditions=firstName="' + $($data.firstname) + '" AND lastName="' + $($data.lastname) + '"&company/id=' + $companyid
    }
    elseif ($query -eq 'Email')
    {
        # 2 things to note: 
        # 1. We should use equal query here for email as this is uniq identifier. With 'LIKE' query we may catch many similar emails:
        # like '%name@domain.com%' will get emails name@domain.com, myname@domain.com, name@domain.com.au ...
        # 2. For the LIKE query - if the searched value starts with a number - it may conflict with url encoding later once transfered over HTTP, so the
        # use of '%25' (which is actual url encoded value of '%') encouraged. 
        $querystring = '?childconditions=communicationItems/value="' + $($email) + '" AND communicationItems/communicationType="Email"' + '&conditions=company/id=' + $companyid
    }
    else
    {
        if ($Global:MxSDebug) { Write-Host '========================= just before returning null' }
        return $null
    }if ($Global:MxSDebug) { Write-Host "========================= querystring: $querystring" }

    
    $success = $false
    $WaitTime = 30
    $RetryCount = 0    
    $RetryCodes = @(503, 504, 520, 521, 522, 524)
    $FailCodes = @(400, 404)
    while ($RetryCount -lt $retry -and $success -eq $false)
    {
        try
        {
            $request = [System.Net.HttpWebRequest]::Create("$CW_Api_Url/apis/3.0/company/contacts$querystring")
        
            $request.Method = 'GET';
            $request.ContentType = 'application/json';
            $authBytes = [System.Text.Encoding]::UTF8.GetBytes($CW_Api_Token);
            $authStr = 'Basic ' + $([System.Convert]::ToBase64String($authBytes));
            $request.Headers['Authorization'] = $authStr;
            $request.Headers['clientId'] = $CW_Api_Client_Id;
            $request.Timeout = 10000
                
            $response = $request.GetResponse();
            $reader = New-Object System.IO.StreamReader $response.GetResponseStream();
            $jsonResult = $reader.ReadToEnd();
            $response.Dispose();

            if ($Global:MxSDebug) { Write-Host "========================= JSON result from WEB(GetContact): $jsonResult" }
            [array]$returnedObject = $jsonResult | ConvertFrom-Json
            if ($Global:MxSDebug) { Write-Host "`r`n===================== Count of found objects: $($returnedObject.Count) `r`n" }
            if ($returnedObject.Count -gt 1)
            {
                foreach ($contactItem in $returnedObject)
                {
                    if ($Global:MxSDebug) { Write-Host "=====================ContactItem===> $($contactItem)" }
                    if ($Global:MxSDebug) { Write-Host "=====================ContactItem>>>> $($contactItem.id)" }
                    if ($Global:MxSDebug) { Write-Host "=====================ContactItem>>>> $($contactItem.firstName) Compare to $($userObjectToAction.firstName)" }
                    if ($Global:MxSDebug) { Write-Host "=====================ContactItem>>>> $($contactItem.lastName) Compare to $($userObjectToAction.lastName)" }
                    If (($contactItem.firstName -eq $userObjectToAction.firstName) -and ($contactItem.lastName -eq $userObjectToAction.lastName))
                    {
                        $success = $true
                        return $( $contactItem  )
                    }
                }
                $success = $true
                return $( $jsonResult | ConvertFrom-Json)
            }
            else
            {
                $success = $true
                return $( $jsonResult | ConvertFrom-Json)
            }
    
        }
        catch
        {
            if ($Global:MxSDebug) { Write-Host "========================= WARNING: $($_.Exception.Message)" }
            # Write-Host "========================= WARNING: $( ConvertTo-json $_.Exception)"
            # Geeting the actual numeric value for the error code.
            # When we run through MxS Env we will get nested InnerException inside the 
            # parent InnerException as we are utilising a HTTP WebClient Wrapper on top
            # of the MxS environment
            if ( Test-Path variable:global:psISE )
            {
                if ($Global:MxSDebug) { Write-Host '==================Running package locally for debugging===========' }
                $ErrorCode = $_.Exception.InnerException.Response.StatusCode.value__
            }
            else
            {
                if ($Global:MxSDebug) { Write-Host '========================= Running package in MxS' }
                $ErrorCode = $_.Exception.InnerException.InnerException.Response.StatusCode.value__
            }
            if ($Global:MxSDebug) { Write-Host "========================= Errorcode: $ErrorCode" }
            # Checking if we got any of Fail Codes
            if ($ErrorCode -in $FailCodes)
            {
                # Setting the variables to make activity Fail
                $success = $false;
                $activityOutput.success = $false;
                # If we need immediate stop - we can uncomment below.
                # Write-Error "CRITICAL: $($_.Exception.Message)" -ErrorAction Stop
                Write-Warning "Warning: $($_.Exception.Message)"
                return $null;
            }
            if ($ErrorCode -in $RetryCodes)
            {
                $RetryCount++

                if ($RetryCount -eq $retry)
                {
                    if ($Global:MxSDebug) { Write-Host '========================= WARNING: Retry limit reached.' }
                }
                else
                {
                    if ($Global:MxSDebug) { Write-Host "========================= Waiting $WaitTime seconds." }
                    Start-Sleep -Seconds $WaitTime
                    if ($Global:MxSDebug) { Write-Host '========================= Retrying.' }
                }

            }
            else
            {
                return $null;
            }
        }
    }
}