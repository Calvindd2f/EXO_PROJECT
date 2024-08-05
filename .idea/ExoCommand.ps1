Function ExoCommand($conn, $command, [HashTable]$cargs, $retryCount = 5)
{
    $success = $false
    $count = 0
    
    $body = @{
        CmdletInput = @{
            CmdletName = "$command"
        }
    }

    if ($cargs -ne $null)
    {
        $body.CmdletInput += @{Parameters = [HashTable]$cargs }
    }

    $json = $body | ConvertTo-Json -Depth 5 -Compress
    [string]$commandFriendly = $($body.CmdletInput.CmdletName)

    for ([int]$x = 0 ; $x -le $($body.CmdletInput.Parameters.Count - 1); $x++)
    {
        try { $param = " -$([Array]$($body.CmdletInput.Parameters.Keys).Item($x))" }catch { $param = '' }
        try { $value = "`"$([Array]$($body.CmdletInput.Parameters.Values).Item($x) -join ',')`"" }catch { $value = '' }
        $commandFriendly += $("$param $value").TrimEnd()
    }
    [System.Console]::WriteLine("Executing: $commandFriendly")
    [System.Console]::WriteLine($json)
    
    [string]$url = $("https://outlook.office365.com/adminapi/beta/$tenant_name/InvokeCommand")
    if (![string]::IsNullOrEmpty($Properties))
    {
        $url = $url + $('?%24select=' + $($Properties.Trim().Replace(' ', '')))
    }
    [Array]$Data = @()
    do
    {
        try
        {
            do
            {
           

                ## Using HTTPWebRequest library

                $request = [System.Net.HttpWebRequest]::Create($url)
                $request.Method = 'POST';
                $request.ContentType = 'application/json;odata.metadata=minimal;odata.streaming=true;';
                $request.Headers['Authorization'] = "Bearer $($exo_token)"
                $request.Headers['x-serializationlevel'] = 'Partial'
                #$request.Headers["x-clientmoduleversion"] = "2.0.6-Preview6"
                $request.Headers['X-AnchorMailbox'] = $("UPN:SystemMailbox{bb558c35-97f1-4cb9-8ff7-d53741dc928c}@$tenant_name")
                $request.Headers['X-prefer'] = 'odata.maxpagesize=1000'
                #$request.Headers["Prefer"] = 'odata.include-annotations="display.*"'
                $request.Headers['X-ResponseFormat'] = 'json' ## Can also be 'clixml'
                $request.Headers['connection-id'] = "$conn_id"
                #$request.Headers["accept-language"] = "en-GB"
                $request.Headers['accept-charset'] = 'UTF-8'
                #$request.Headers["preference-applied"] = ''
                $request.Headers['warningaction'] = ''
                $request.SendChunked = $true;
                $request.TransferEncoding = 'gzip'
                $request.UserAgent = 'Mozilla/5.0 (Windows NT; Windows NT 10.0; en-AU) WindowsPowerShell/5.1.19041.1682'
                #$request.Host = "outlook.office365.com"
                $request.Accept = 'application/json'
                $request.Timeout = $($defaultTimeout * 1000)

                $requestWriter = New-Object System.IO.StreamWriter $request.GetRequestStream();
                $requestWriter.Write($json);
                $requestWriter.Flush();
                $requestWriter.Close();
                $requestWriter.Dispose()

                $response = $request.GetResponse();
                $reader = New-Object System.IO.StreamReader $response.GetResponseStream();
                $jsonResult = $reader.ReadToEnd();
                $result = $(ConvertFrom-Json $jsonResult)
                $response.Dispose();

                if (@($result.value).Count -ne 0)
                {
                    $Data += $($result.value)
                    [System.Console]::WriteLine("Got $($result.value.Count) items")
                }
                try { $url = $result.'@odata.nextLink' }catch { $url = '' }
                if (![string]::IsNullOrEmpty($url))
                {
                    [System.Console]::WriteLine('Getting next page...')
                }
            }while (![string]::IsNullOrEmpty($url))
            $success = $true
            $count = $retry
            return @($Data)
        }
        catch
        {

            if ($($_.Exception.Message) -like '*timed out*' -or $($_.Exception.Message) -like '*Unable to connect to the remote server*')
            {
                $count++
                [System.Console]::WriteLine('WARNING: TIMEOUT: Will retry in 10 seconds.')
                Start-Sleep -Seconds 10
                if ($count -gt $retry) { throw 'Timeout retry limit reached' }
            }
            else
            {
                [System.Console]::WriteLine("WARNING: Failed to execute Exchange command: $commandFriendly")
                [System.Console]::WriteLine("WARNING: $($_.Exception.Message)")
                throw;
            }
        }
    }while ($count -lt $retry -or $success -eq $false)
    return $null
}