using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ExoCommand
{
    public static async Task<List<object>> ExecuteAsync(string conn, string command, Dictionary<string, object> cargs = null, int retryCount = 5)
    {
        bool success = false;
        int count = 0;
        string tenantName = "your_tenant_name"; // Replace w/ actual tenant name
        string exoToken = "your_exo_token";     // Replace w/ actual token           [OG .onmicrosoft.com email for tenant]
        string connId = "your_conn_id";         // Replace w/ actual connection ID   [can be random GUID]
        int defaultTimeout = 1000;              // Adjust as necessary 

        var body = new
        {
            CmdletInput = new
            {
                CmdletName = command,
                Parameters = cargs
            }
        };

        string json = JsonSerializer.Serialize(body);
        string commandFriendly = BuildCommandFriendly(body);

        Console.WriteLine($"Executing: {commandFriendly}");
        Console.WriteLine(json);

        string url = $"https://outlook.office365.com/adminapi/beta/{tenantName}/InvokeCommand";
        
        List<object> data = new List<object>();

        do
        {
            try
            {
                do
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/json;odata.metadata=minimal;odata.streaming=true;";
                    request.Headers["Authorization"] = $"Bearer {exoToken}";
                    request.Headers["x-serializationlevel"] = "Partial";
                    request.Headers["X-AnchorMailbox"] = $"UPN:SystemMailbox{{bb558c35-97f1-4cb9-8ff7-d53741dc928c}}@{tenantName}";
                    request.Headers["X-prefer"] = "odata.maxpagesize=1000";
                    request.Headers["X-ResponseFormat"] = "json";
                    request.Headers["connection-id"] = connId;
                    request.Headers["accept-charset"] = "UTF-8";
                    request.Headers["warningaction"] = "";
                    request.SendChunked = true;
                    request.UserAgent = "Mozilla/5.0 (Windows NT; Windows NT 10.0; en-NL) WindowsPowerShell/5.1.19041.1682";
                    request.Accept = "application/json";
                    request.Timeout = defaultTimeout * 1000;

                    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                    {
                        streamWriter.Write(json);
                    }

                    using (var response = (HttpWebResponse)request.GetResponse())
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string jsonResult = reader.ReadToEnd();
                        var result = JsonSerializer.Deserialize<ResponseData>(jsonResult);

                        if (result.Value != null && result.Value.Count > 0)
                        {
                            data.AddRange(result.Value);
                            Console.WriteLine($"Got {result.Value.Count} items");
                        }

                        url = result.ODataNextLink;
                        if (!string.IsNullOrEmpty(url))
                        {
                            Console.WriteLine("Getting next page...");
                        }
                    }
                } while (!string.IsNullOrEmpty(url));

                success = true;
                count = retryCount;
                return data;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("timed out") || ex.Message.Contains("Unable to connect to the remote server"))
                {
                    count++;
                    Console.WriteLine("WARNING: TIMEOUT: Will retry in 10 seconds.");
                    await Task.Delay(10000);
                    if (count > retryCount) throw new Exception("Timeout retry limit reached");
                }
                else
                {
                    Console.WriteLine($"WARNING: Failed to execute Exchange command: {commandFriendly}");
                    Console.WriteLine($"WARNING: {ex.Message}");
                    throw;
                }
            }
        } while (count < retryCount || !success);

        return null;
    }

    private static string BuildCommandFriendly(object body)
    {
        var cmdletInput = (JsonElement)((JsonElement)body).GetProperty("CmdletInput");
        string commandFriendly = cmdletInput.GetProperty("CmdletName").GetString();

        if (cmdletInput.TryGetProperty("Parameters", out JsonElement parameters))
        {
            foreach (var param in parameters.EnumerateObject())
            {
                commandFriendly += $" -{param.Name} \"{param.Value}\"";
            }
        }

        return commandFriendly.TrimEnd();
    }
}

public class ResponseData
{
    public List<object> Value { get; set; }
    public string ODataNextLink { get; set; }
}
