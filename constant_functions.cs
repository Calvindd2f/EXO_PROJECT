using System;
using System.Management.Automation;
using System.Collections.Generic;
using Microsoft.Exchange.Management.ExoPowershellSnapin;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Net.Http;

//region constant functions

public enum Column
{
    CmdletName,
    CmdletParameters,
    StartTime,
    EndTime,
    GenericInfo,
    GenericError
}

[Cmdlet("Log-Column")]
public class LogColumn : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public Column ColumnName { get; set; }

    [Parameter(Mandatory = true)]
    public string CmdletId { get; set; }

    [Parameter(Mandatory = true)]
    public object Value { get; set; }

    protected override void ProcessRecord()
    {
        var connectionContext = GetCurrentConnectionContext();
        if (connectionContext == null)
        {
            return;
        }

        var logger = connectionContext.Logger;
        if (logger == null)
        {
            return;
        }

        switch (ColumnName)
        {
            case Column.StartTime:
                if (!(Value is DateTime))
                {
                    throw new ArgumentException("StartTime must be a DateTime value");
                }
                logger.LogStartTime(CmdletId, (DateTime)Value);
                break;

            case Column.EndTime:
                if (!(Value is DateTime))
                {
                    throw new ArgumentException("EndTime must be a DateTime value");
                }
                logger.LogEndTime(CmdletId, (DateTime)Value);
                break;

            case Column.CmdletName:
                if (!(Value is string))
                {
                    throw new ArgumentException("CmdletName must be a string value");
                }
                logger.LogCmdletName(CmdletId, (string)Value);
                break;

            case Column.CmdletParameters:
                if (!(Value is string || Value is Dictionary<string, object>))
                {
                    throw new ArgumentException("CmdletParameters must be a string or dictionary value");
                }
                logger.LogCmdletParameters(CmdletId, Value);
                break;

            case Column.GenericInfo:
                if (!(Value is string))
                {
                    throw new ArgumentException("GenericInfo must be a string value");
                }
                logger.LogGenericInfo(CmdletId, (string)Value);
                break;

            case Column.GenericError:
                if (!(Value is string || Value is ErrorRecord))
                {
                    if (Value != null && Value.ToString() != null)
                    {
                        Value = Value.ToString();
                    }
                    else
                    {
                        throw new ArgumentException("GenericError must have ToString() or ErrorRecord value");
                    }
                }
                logger.LogGenericError(CmdletId, Value);
                break;

            default:
                throw new ArgumentException("Invalid ColumnName");
        }
    }

    private ConnectionContext GetCurrentConnectionContext()
    {
        var getCmdlet = new GetCurrentConnectionContext();
        getCmdlet.CommandRuntime = this.CommandRuntime;
        var results = new List<object>();
        getCmdlet.InvokeCommand(results);
        return results.Count > 0 ? results[0] as ConnectionContext : null;
    }
}

[Cmdlet("Get", "CurrentConnectionContext")]
[OutputType(typeof(ConnectionContext))]
public class GetCurrentConnectionContext : PSCmdlet
{
    [Parameter(Mandatory = false)]
    public Guid ConnectionContextObjectId { get; set; }

    protected override void ProcessRecord()
    {
        if (ConnectionContextObjectId != Guid.Empty)
        {
            WriteObject(ConnectionContextFactory.GetCurrentConnectionContext(ConnectionContextObjectId));
        }
        else
        {
            WriteObject(null);
        }
    }
}

[Cmdlet("Get", "SortableDateTimePattern")]
[OutputType(typeof(string))]
public class GetSortableDateTimePattern : PSCmdlet
{
    protected override void ProcessRecord()
    {
        var cultureInfo = GetCulture();
        if (cultureInfo != null)
        {
            var cultureDateTimeFormat = cultureInfo.DateTimeFormat;
            if (cultureDateTimeFormat != null)
            {
                var sortableCultureDateTimeFormat = cultureDateTimeFormat.SortableDateTimePattern;
                if (sortableCultureDateTimeFormat != null)
                {
                    WriteObject(sortableCultureDateTimeFormat);
                    return;
                }
            }
        }
        WriteObject("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
    }

    private System.Globalization.CultureInfo GetCulture()
    {
        // Implement logic to get current culture, possibly using PowerShell's Get-Culture cmdlet
        throw new NotImplementedException("GetCulture needs to be implemented");
    }
}

[Cmdlet(VerbsData.Init, "Log")]
public class InitLog : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string CmdletId { get; set; }

    protected override void ProcessRecord()
    {
        var connectionContext = GetCurrentConnectionContext();
        if (connectionContext == null)
        {
            return;
        }

        var logger = connectionContext.Logger;
        if (logger != null)
        {
            logger.InitLog(CmdletId);
        }
    }
}

[Cmdlet(VerbsData.Commit, "Log")]
public class CommitLog : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string CmdletId { get; set; }

    protected override void ProcessRecord()
    {
        var connectionContext = GetCurrentConnectionContext();
        if (connectionContext == null)
        {
            return;
        }

        var logger = connectionContext.Logger;
        if (logger != null)
        {
            logger.CommitLog(CmdletId);
        }
    }
}

[Cmdlet(VerbsCommon.Get, "ModuleVersion")]
public class GetModuleVersion : PSCmdlet
{
    private static string moduleVersion;

    protected override void ProcessRecord()
    {
        if (!string.IsNullOrEmpty(moduleVersion))
        {
            WriteVerbose($"Returning precomputed version info: {moduleVersion}");
            WriteObject(moduleVersion);
            return;
        }

        var exoModule = GetExchangeOnlineModule();
        if (exoModule == null)
        {
            WriteError(new ErrorRecord(new Exception("ExchangeOnlineManagement module not found"), "ModuleNotFound", ErrorCategory.ObjectNotFound, null));
            return;
        }

        moduleVersion = exoModule.Version.ToString();

        var exoModuleRoot = Path.GetDirectoryName(Path.GetDirectoryName(exoModule.Path));
        var exoModuleManifestPath = Path.Combine(exoModuleRoot, "ExchangeOnlineManagement.psd1");

        if (!File.Exists(exoModuleManifestPath))
        {
            WriteVerbose($"Module manifest path invalid, path: {exoModuleManifestPath}, skipping extracting prerelease info");
            WriteObject(moduleVersion);
            return;
        }

        var manifestContent = File.ReadAllText(exoModuleManifestPath);
        var match = Regex.Match(manifestContent, @"Prerelease\s*=\s*'([^']*)'");
        if (match.Success)
        {
            moduleVersion = $"{exoModule.Version}-{match.Groups[1].Value.Trim()}";
        }

        WriteVerbose($"Computed version info: {moduleVersion}");
        WriteObject(moduleVersion);
    }

    private PSModuleInfo GetExchangeOnlineModule()
    {
        var exoModule = InvokeCommand.InvokeScript("Get-Module ExchangeOnlineManagement").FirstOrDefault() as PSObject;
        if (exoModule != null)
        {
            return exoModule.BaseObject as PSModuleInfo;
        }

        var connectExoCommand = InvokeCommand.InvokeScript("Get-Command -Name Connect-ExchangeOnline").FirstOrDefault() as PSObject;
        if (connectExoCommand != null)
        {
            return (connectExoCommand.Properties["Module"].Value as PSObject)?.BaseObject as PSModuleInfo;
        }

        return null;
    }
}

[Cmdlet(VerbsCommon.Get, "ClaimsValueFromHeader")]
public class GetClaimsValueFromHeader : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public object AuthenticationHeader { get; set; }

    protected override void ProcessRecord()
    {
        var result = TokenProviderUtils.ExtractClaimsIfPresentFromAuthenticationHeader(AuthenticationHeader);
        WriteObject(result);
    }
}

[Cmdlet(VerbsCommon.Get, "AuthInfoUsingTokenProvider")]
public class GetAuthInfoUsingTokenProvider : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public object TokenProviderObjectId { get; set; }

    protected override void ProcessRecord()
    {
        var result = AuthHeaderUtils.GetAuthHeader(TokenProviderObjectId);
        WriteObject(result);
    }
}

[Cmdlet(VerbsSecurity.Protect, "Value")]
public class ProtectValue : PSCmdlet
{
    [Parameter(Mandatory = true)]
    [AllowEmptyString]
    [AllowNull]
    public string UnsecureString { get; set; }

    private static string PublicKey; // This should be set somewhere in your module initialization

    protected override void ProcessRecord()
    {
        if (string.IsNullOrEmpty(PublicKey))
        {
            ThrowTerminatingError(new ErrorRecord(
                new InvalidOperationException("Required resource is not available to continue execution. Please re-establish the connection to continue."),
                "PublicKeyNotAvailable",
                ErrorCategory.ResourceUnavailable,
                null));
        }

        if (!string.IsNullOrEmpty(UnsecureString))
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(PublicKey);
                byte[] bytes = Encoding.UTF8.GetBytes(UnsecureString);
                byte[] encryptedBytes = rsa.Encrypt(bytes, false);
                string result = Convert.ToBase64String(encryptedBytes);
                WriteObject(result);
            }
        }
        else
        {
            WriteObject(UnsecureString);
        }
    }
}

[Cmdlet(VerbsCommon.Get, "ErrorMessage")]
public class GetErrorMessage : PSCmdlet
{
    [Parameter(Mandatory = true)]
    [AllowNull]
    public ErrorRecord ErrorObject { get; set; }

    protected override void ProcessRecord()
    {
        if (ErrorObject?.ErrorDetails?.Message == null)
        {
            WriteObject(null);
            return;
        }

        try
        {
            var errorDetails = JObject.Parse(ErrorObject.ErrorDetails.Message);
            string message = null;

            if (errorDetails["error"]?["details"] is JArray details && details.Count > 0)
            {
                message = details[0]["message"]?.ToString();
            }
            else if (errorDetails["error"]?["innererror"]?["internalexception"]?["message"] != null)
            {
                message = errorDetails["error"]["innererror"]["internalexception"]["message"].ToString();
            }

            WriteObject(message);
        }
        catch
        {
            WriteObject(null);
        }
    }
}

public class ErrorHandlingCmdlets
{
    [Cmdlet(VerbsCommon.Get, "ErrorId")]
    public class GetErrorId : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [AllowNull]
        public ErrorRecord ErrorObject { get; set; }

        protected override void ProcessRecord()
        {
            var errorId = new List<string>();
            if (ErrorObject?.Exception?.Response is HttpResponseMessage response)
            {
                var headers = response.Headers;
                if (headers.TryGetValues("X-CalculatedBETarget", out var beTargetValues))
                {
                    var serverName = beTargetValues.FirstOrDefault();
                    if (serverName != null)
                    {
                        var dotIndex = serverName.IndexOf('.');
                        if (dotIndex >= 0)
                            serverName = serverName.Substring(0, dotIndex);
                        errorId.Add($"Server={serverName}");
                    }
                }
                if (headers.TryGetValues("request-id", out var requestIdValues))
                    errorId.Add($"RequestId={requestIdValues.FirstOrDefault()}");
                if (headers.TryGetValues("Date", out var dateValues))
                    errorId.Add($"TimeStamp={dateValues.FirstOrDefault()}");
            }

            WriteObject($"[{string.Join(",", errorId)}]");
        }
    }

    [Cmdlet(VerbsCommon.Get, "ErrorCategoryInfo")]
    public class GetErrorCategoryInfo : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [AllowNull]
        public ErrorRecord ErrorObject { get; set; }

        protected override void ProcessRecord()
        {
            var errorCategoryInfo = new PSObject();
            errorCategoryInfo.Properties.Add(new PSNoteProperty("Activity", SessionState.PSVariable.GetValue("CmdletName")));
            errorCategoryInfo.Properties.Add(new PSNoteProperty("Category", ErrorCategory.NotSpecified));
            errorCategoryInfo.Properties.Add(new PSNoteProperty("Reason", "Exception"));
            errorCategoryInfo.Properties.Add(new PSNoteProperty("TargetName", null));
            errorCategoryInfo.Properties.Add(new PSNoteProperty("TargetType", null));

            if (ErrorObject?.ErrorDetails?.Message != null)
            {
                try
                {
                    var errorDetails = JObject.Parse(ErrorObject.ErrorDetails.Message);
                    var details = errorDetails["error"]?["details"]?.FirstOrDefault();
                    if (details != null)
                    {
                        errorCategoryInfo.Properties["TargetName"].Value = details["target"]?.ToString();
                        var errorCategory = details["code"]?.ToString();
                        if (int.TryParse(errorCategory, out int categoryInt) && Enum.IsDefined(typeof(ErrorCategory), categoryInt))
                            errorCategoryInfo.Properties["Category"].Value = (ErrorCategory)categoryInt;
                    }
                }
                catch { /* Ignore parsing errors */ }
            }

            if (ErrorObject?.Exception?.Response is HttpResponseMessage response)
            {
                var headers = response.Headers;
                if (SessionState.PSVariable.GetValue("UseBatching") as bool? == true)
                    errorCategoryInfo.Properties["Activity"].Value = headers.GetValues("Cmdlet-Name").FirstOrDefault();
                errorCategoryInfo.Properties["Reason"].Value = headers.GetValues("x-ExceptionType").FirstOrDefault() ?? "Exception";
                errorCategoryInfo.Properties["TargetType"].Value = headers.GetValues("x-ExceptionTargetType").FirstOrDefault();
            }

            WriteObject(errorCategoryInfo);
        }
    }

    [Cmdlet(VerbsCommon.Get, "ExceptionObject")]
    public class GetExceptionObject : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [AllowNull]
        public string ErrorMessageObject { get; set; }

        protected override void ProcessRecord()
        {
            Exception customException;
            try
            {
                var errorMessageObj = JObject.Parse(ErrorMessageObject);
                var errorId = errorMessageObj["localizedString"]?["stringId"]?.ToString() ?? "";
                var className = errorMessageObj["ClassName"]?.ToString() ?? "";
                var message = errorMessageObj["Message"]?.ToString() ?? "";
                var formattedErrorMessage = $"{errorId}|{className}|{message}";
                customException = new Exception(formattedErrorMessage);
                customException.Data["RemoteException"] = errorMessageObj;
            }
            catch
            {
                customException = new Exception(ErrorMessageObject);
            }

            WriteObject(customException);
        }
    }

    [Cmdlet(VerbsCommunications.Write, "ErrorMessage")]
    public class WriteErrorMessage : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [AllowNull]
        public ErrorRecord ErrorObject { get; set; }

        protected override void ProcessRecord()
        {
            if (ErrorObject?.Exception?.Response is HttpResponseMessage response
                && ErrorObject.ErrorDetails?.Message != null
                && response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                WriteError(new ErrorRecord(new Exception(ErrorObject.ErrorDetails.Message), "ThrottlingError", ErrorCategory.LimitsExceeded, null));
                return;
            }

            var getErrorMessage = new GetErrorMessage();
            getErrorMessage.ErrorObject = ErrorObject;
            getErrorMessage.CommandRuntime = this.CommandRuntime;
            var errorMessages = new List<object>();
            getErrorMessage.InvokeCommand(errorMessages);
            var errorMessage = errorMessages.FirstOrDefault() as string;

            if (errorMessage != null)
            {
                var getExceptionObject = new GetExceptionObject();
                getExceptionObject.ErrorMessageObject = errorMessage;
                getExceptionObject.CommandRuntime = this.CommandRuntime;
                var exceptions = new List<object>();
                getExceptionObject.InvokeCommand(exceptions);
                var customException = exceptions.FirstOrDefault() as Exception;

                var getErrorId = new GetErrorId();
                getErrorId.ErrorObject = ErrorObject;
                getErrorId.CommandRuntime = this.CommandRuntime;
                var errorIds = new List<object>();
                getErrorId.InvokeCommand(errorIds);
                var customErrorId = errorIds.FirstOrDefault() as string;

                var getErrorCategoryInfo = new GetErrorCategoryInfo();
                getErrorCategoryInfo.ErrorObject = ErrorObject;
                getErrorCategoryInfo.CommandRuntime = this.CommandRuntime;
                var categoryInfos = new List<object>();
                getErrorCategoryInfo.InvokeCommand(categoryInfos);
                var customCategoryInfo = categoryInfos.FirstOrDefault() as PSObject;

                var customErrorObject = new ErrorRecord(
                    customException,
                    customErrorId,
                    (ErrorCategory)customCategoryInfo.Properties["Category"].Value,
                    customCategoryInfo.Properties["TargetName"].Value);
                customErrorObject.CategoryInfo.Activity = customCategoryInfo.Properties["Activity"].Value as string;
                customErrorObject.CategoryInfo.Reason = customCategoryInfo.Properties["Reason"].Value as string;
                customErrorObject.CategoryInfo.TargetType = customCategoryInfo.Properties["TargetType"].Value as string;

                var errorString = $"{errorMessage} : {customErrorId} : {customCategoryInfo.Properties["Reason"].Value}";
                // Assuming Log-Column is available as a cmdlet
                var logColumn = InvokeCommand.GetCommand("Log-Column", CommandTypes.Cmdlet) as PSCmdlet;
                if (logColumn != null)
                {
                    logColumn.Parameters.Add("ColumnName", "GenericError");
                    logColumn.Parameters.Add("CmdletId", SessionState.PSVariable.GetValue("CmdletRequestId"));
                    logColumn.Parameters.Add("Value", errorString);
                    logColumn.Invoke();
                }

                WriteError(customErrorObject);
            }
            else
            {
                WriteError(new ErrorRecord(new Exception("A server side error has occurred because of which the operation could not be completed. Please try again after some time. If the problem still persists, please reach out to MS support."), "ServerSideError", ErrorCategory.ConnectionError, null));
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "ErrorMessage")]
    public class GetErrorMessage : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [AllowNull]
        public ErrorRecord ErrorObject { get; set; }

        protected override void ProcessRecord()
        {
            if (ErrorObject?.ErrorDetails?.Message == null)
            {
                WriteObject(null);
                return;
            }

            try
            {
                var errorDetails = JObject.Parse(ErrorObject.ErrorDetails.Message);
                string message = null;

                if (errorDetails["error"]?["details"] is JArray details && details.Count > 0)
                {
                    message = details[0]["message"]?.ToString();
                }
                else if (errorDetails["error"]?["innererror"]?["internalexception"]?["message"] != null)
                {
                    message = errorDetails["error"]["innererror"]["internalexception"]["message"].ToString();
                }

                WriteObject(message);
            }
            catch
            {
                WriteObject(null);
            }
        }
    }
}

public class UtilityCmdlets
{
    [Cmdlet(VerbsCommon.Get, "SanitizedBoolValuesInFilter")]
    public class SanitizeBoolValuesInFilter : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        [AllowNull]
        [AllowEmptyString]
        public string Val { get; set; }

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(Val))
            {
                WriteObject(Val);
                return;
            }

            string result = Regex.Replace(Val, @"\$true", "'True'", RegexOptions.IgnoreCase);
            result = Regex.Replace(result, @"\$false", "'False'", RegexOptions.IgnoreCase);
            WriteObject(result);
        }
    }

    [Cmdlet(VerbsCommon.Get, "ClaimsFromExceptionDetails")]
    public class GetClaimsFromExceptionDetails : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public ErrorRecord ErrorObject { get; set; }

        protected override void ProcessRecord()
        {
            string claims = string.Empty;
            if (ErrorObject?.Exception?.Response is HttpWebResponse response)
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    string authenticationHeader = response.Headers["WWW-Authenticate"];
                    if (!string.IsNullOrEmpty(authenticationHeader))
                    {
                        // Assuming Get-ClaimsValueFromHeader is available as a cmdlet
                        var getClaimsValueFromHeader = InvokeCommand.GetCommand("Get-ClaimsValueFromHeader", CommandTypes.Cmdlet) as PSCmdlet;
                        if (getClaimsValueFromHeader != null)
                        {
                            getClaimsValueFromHeader.Parameters.Add("AuthenticationHeader", authenticationHeader);
                            var results = new PSDataCollection<PSObject>();
                            getClaimsValueFromHeader.Invoke<PSObject, PSObject>(null, results);
                            claims = results.Count > 0 ? results[0].ToString() : string.Empty;
                        }
                    }
                }
            }
            WriteObject(claims);
        }
    }

    [Cmdlet(VerbsCommon.Get, "WaitTimeForRetry")]
    public class GetWaitTimeForRetry : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public ErrorRecord ErrorObject { get; set; }

        [Parameter(Mandatory = true)]
        public int RetryCount { get; set; }

        protected override void ProcessRecord()
        {
            // Assuming RetryableExceptions and RetryableHTTPResponses are available as variables
            var retryableExceptions = SessionState.PSVariable.GetValue("RetryableExceptions") as string[];
            var retryableHTTPResponses = SessionState.PSVariable.GetValue("RetryableHTTPResponses") as HttpStatusCode[];

            if ((retryableExceptions?.Length ?? 0) == 0 && (retryableHTTPResponses?.Length ?? 0) == 0)
            {
                WriteVerbose("Permanent Exception occurred: this request will not be retried.");
                WriteObject(-1);
                return;
            }

            if (ErrorObject?.Exception?.Response == null || ErrorObject.ErrorDetails == null)
            {
                WriteVerbose("Unknown Exception occurred: this request will not be retried.");
                WriteObject(-1);
                return;
            }

            var errorStatusCode = (ErrorObject.Exception.Response as HttpWebResponse)?.StatusCode;
            var errorDetails = JObject.Parse(ErrorObject.ErrorDetails.Message);
            var errorDetailsMessage = errorDetails["error"]?["details"]?[0]?["message"]?.ToString() ?? "";

            int waitTimeForRetry = -1;

            if (retryableHTTPResponses?.Contains(errorStatusCode) ?? false)
            {
                waitTimeForRetry = 0;
                WriteVerbose($"Exception occurred: HTTP status code {errorStatusCode}; Request is being retried.");
            }

            if (errorStatusCode == (HttpStatusCode)429)
            {
                var retryAfter = ErrorObject.Exception.Response.Headers["Retry-After"];
                if (string.IsNullOrEmpty(retryAfter))
                {
                    waitTimeForRetry = 0;
                    WriteVerbose("Request has been throttled, but the response header does not contain any retry wait time.");
                }
                else
                {
                    waitTimeForRetry = (int)(double.Parse(retryAfter) * 1000);
                    WriteVerbose($"The current request is throttled at the server, waiting for {waitTimeForRetry} milliseconds");
                }
            }

            if (waitTimeForRetry == -1)
            {
                foreach (var exceptionName in retryableExceptions ?? Array.Empty<string>())
                {
                    if (errorDetailsMessage.Contains(exceptionName))
                    {
                        waitTimeForRetry = 0;
                        WriteVerbose($"Exception occurred: Exception {exceptionName}; Request is being retried.");
                        break;
                    }
                }
            }

            if (waitTimeForRetry == 0)
            {
                var defaultRetryInterval = (int)SessionState.PSVariable.GetValue("DefaultRetryIntervalInMilliSeconds");
                var defaultRetryIntervalBase = (double)SessionState.PSVariable.GetValue("DefaultRetryIntervalExponentBase");
                waitTimeForRetry = (int)(defaultRetryInterval * Math.Pow(defaultRetryIntervalBase, RetryCount));
                WriteVerbose("Using exponential backoff retry logic to get the wait time for next retry");
            }

            WriteObject(waitTimeForRetry);
        }
    }

    [Cmdlet(VerbsData.ConvertTo, "HashTable")]
    public class ConvertToHashTable : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Hashtable Parameter { get; set; }

        protected override void ProcessRecord()
        {
            if (!Parameter.ContainsKey("@odata.type"))
            {
                Parameter.Add("@odata.type", "#Exchange.GenericHashTable");
            }

            foreach (DictionaryEntry entry in Parameter)
            {
                if (entry.Value is Hashtable nestedHashtable)
                {
                    Parameter[entry.Key] = ConvertNestedHashtable(nestedHashtable);
                }
            }

            WriteObject(Parameter);
        }

        private Hashtable ConvertNestedHashtable(Hashtable hashtable)
        {
            var convertToHashTable = new ConvertToHashTable();
            convertToHashTable.Parameter = hashtable;
            convertToHashTable.CommandRuntime = this.CommandRuntime;
            var results = new PSDataCollection<PSObject>();
            convertToHashTable.Invoke<PSObject, PSObject>(null, results);
            return results.Count > 0 ? results[0].BaseObject as Hashtable : hashtable;
        }
    }

    [Cmdlet(VerbsData.ConvertTo, "ByteArrayCollection")]
    public class ConvertToByteArrayCollection : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public byte[][] Parameter { get; set; }

        protected override void ProcessRecord()
        {
            var newParameter = new ArrayList();
            foreach (var value in Parameter)
            {
                var byteArray = new Hashtable();
                byteArray.Add("Data", value);
                newParameter.Add(byteArray);
            }
            WriteObject(newParameter);
        }
    }

    [Cmdlet(VerbsData.ConvertTo, "ByteArray")]
    public class ConvertToByteArray : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public byte[] Parameter { get; set; }

        protected override void ProcessRecord()
        {
            var newParameter = new Hashtable();
            newParameter.Add("@odata.type", "#Exchange.ByteArrayType");
            newParameter.Add("Data", Parameter);
            WriteObject(newParameter);
        }
    }

    [Cmdlet(VerbsCommon.Get, "ShouldUseCustomRoutingHint")]
    public class ShouldUseCustomRoutingHint : PSCmdlet
    {
        [Parameter]
        public SwitchParameter UserRequestedCustomRouting { get; set; }

        protected override void ProcessRecord()
        {
            bool shouldUseCustomRoutingByDefault = (bool)SessionState.PSVariable.GetValue("ShouldUse-CustomRoutingByDefault");
            bool result = shouldUseCustomRoutingByDefault || UserRequestedCustomRouting.IsPresent;
            WriteObject(result);
        }
    }
}

public class RoutingHintCmdlets
{
    [Cmdlet(VerbsCommon.Get, "MailboxIDBasedRoutingHint")]
    public class GetMailboxIDBasedRoutingHint : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Value { get; set; }

        [Parameter(Mandatory = true)]
        public string OrganizationId { get; set; }

        protected override void ProcessRecord()
        {
            string emailAddressRegexPattern = @"[a-zA-Z0-9!#$%*+\-/?^_`.{|}~]+@([a-zA-Z0-9\-]+\.)+[a-zA-Z]{2,4}";
            if (Regex.IsMatch(Value, emailAddressRegexPattern))
            {
                WriteObject($"SMTP:{Value}");
                return;
            }

            if (Guid.TryParse(Value, out Guid guid))
            {
                WriteObject($"MBX:{guid}@{OrganizationId}");
                return;
            }

            WriteObject(null);
        }
    }

    [Cmdlet(VerbsCommon.Get, "MailboxFolderIDBasedRoutingHint")]
    public class GetMailboxFolderIDBasedRoutingHint : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string Value { get; set; }

        [Parameter(Mandatory = true)]
        public string OrganizationId { get; set; }

        protected override void ProcessRecord()
        {
            string mailboxId = Value.Split(':')[0];
            var getMailboxIdBasedRoutingHint = new GetMailboxIDBasedRoutingHint
            {
                Value = mailboxId,
                OrganizationId = OrganizationId,
                CommandRuntime = this.CommandRuntime
            };

            var results = new PSDataCollection<PSObject>();
            getMailboxIdBasedRoutingHint.Invoke<PSObject, PSObject>(null, results);

            if (results.Count > 0)
            {
                WriteObject(results[0]);
            }
            else
            {
                WriteObject(null);
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "RoutingHint")]
    public class GetRoutingHint : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var connectionContext = SessionState.PSVariable.GetValue("ConnectionContext") as dynamic;
            var authInfo = SessionState.PSVariable.GetValue("AuthInfo") as dynamic;

            if (connectionContext != null)
            {
                string routingHintPrefix = connectionContext.RoutingHintPrefix ?? "UPN";
                string routingHint = connectionContext.RoutingHint ?? authInfo.UserPrincipalName;
                WriteObject($"{routingHintPrefix}:{routingHint}");
            }
            else
            {
                WriteObject($"UPN:{authInfo.UserPrincipalName}");
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "RoutingHintHeaderName")]
    public class GetRoutingHintHeaderName : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            var connectionContext = SessionState.PSVariable.GetValue("ConnectionContext") as dynamic;

            if (connectionContext != null)
            {
                string routingHintHeader = connectionContext.RoutingHintHeader ?? "X-AnchorMailbox";
                WriteObject(routingHintHeader);
            }
            else
            {
                WriteObject("X-AnchorMailbox");
            }
        }
    }
}

public class UtilityCmdletsExtended
{
    [Cmdlet(VerbsLifecycle.Confirm, "ParameterSet")]
    public class ValidateParameterSet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string CmdletName { get; set; }

        [Parameter(Mandatory = true)]
        public Hashtable ParameterList { get; set; }

        protected override void ProcessRecord()
        {
            var cmdletInfo = InvokeCommand.GetCommand(CmdletName, CommandTypes.Cmdlet) as CmdletInfo;
            if (cmdletInfo == null)
            {
                WriteObject(false);
                return;
            }

            var commonParameters = typeof(PSCmdlet).GetProperties()
                .Where(prop => prop.GetCustomAttributes(typeof(ParameterAttribute), true).Any())
                .Select(prop => prop.Name)
                .ToList();

            foreach (var parameterSet in cmdletInfo.ParameterSets)
            {
                bool allParamsPresentInParameterSet = true;
                var parameterSetParams = parameterSet.Parameters.Select(p => p.Name);

                foreach (var paramKey in ParameterList.Keys)
                {
                    if (!parameterSetParams.Contains(paramKey) && !commonParameters.Contains(paramKey))
                    {
                        allParamsPresentInParameterSet = false;
                        break;
                    }
                }

                if (allParamsPresentInParameterSet)
                {
                    bool allMandatoryParametersPresentInParameterSet = true;
                    foreach (var param in parameterSet.Parameters)
                    {
                        if (param.IsMandatory && !ParameterList.ContainsKey(param.Name))
                        {
                            allMandatoryParametersPresentInParameterSet = false;
                            break;
                        }
                    }

                    if (allMandatoryParametersPresentInParameterSet)
                    {
                        WriteObject(true);
                        return;
                    }
                }
            }

            WriteObject(false);
        }
    }

    [Cmdlet(VerbsCommon.Add, "Headers")]
    public class PopulateHeaders : PSCmdlet
    {
        [Parameter]
        public ScriptBlock ParameterBasedRoutingHintDelegate { get; set; }

        [Parameter]
        public string ParameterBasedRoutingHintParameterValue { get; set; }

        [Parameter]
        public SwitchParameter UseParameterBasedRouting { get; set; }

        [Parameter(Mandatory = true)]
        public PSObject AuthInfo { get; set; }

        [Parameter(Mandatory = true)]
        public Hashtable Headers { get; set; }

        protected override void ProcessRecord()
        {
            Headers["Authorization"] = AuthInfo.Properties["AuthorizationHeader"].Value.ToString();

            string routingHint = null;
            string routingHintHeaderName = "X-AnchorMailbox";

            if (!string.IsNullOrWhiteSpace(ParameterBasedRoutingHintParameterValue) && ParameterBasedRoutingHintDelegate != null && UseParameterBasedRouting)
            {
                var results = ParameterBasedRoutingHintDelegate.Invoke(
                    new object[] { ParameterBasedRoutingHintParameterValue, AuthInfo.Properties["TenantId"].Value }
                );
                routingHint = results.Count > 0 ? results[0].ToString() : null;
            }

            if (!string.IsNullOrWhiteSpace(routingHint))
            {
                Headers[routingHintHeaderName] = routingHint;
                Headers["parameter-based-routing"] = true;
            }
            else
            {
                var connectionContext = SessionState.PSVariable.GetValue("ConnectionContext") as PSObject;
                if (connectionContext != null)
                {
                    var getRoutingHint = InvokeCommand.GetCommand("Get-RoutingHint", CommandTypes.Cmdlet) as PSCmdlet;
                    var getRoutingHintHeaderName = InvokeCommand.GetCommand("Get-RoutingHintHeaderName", CommandTypes.Cmdlet) as PSCmdlet;

                    if (getRoutingHint != null && getRoutingHintHeaderName != null)
                    {
                        var routingHintResults = new PSDataCollection<PSObject>();
                        getRoutingHint.Invoke<PSObject, PSObject>(null, routingHintResults);
                        routingHint = routingHintResults.Count > 0 ? routingHintResults[0].ToString() : null;

                        var headerNameResults = new PSDataCollection<PSObject>();
                        getRoutingHintHeaderName.Invoke<PSObject, PSObject>(null, headerNameResults);
                        routingHintHeaderName = headerNameResults.Count > 0 ? headerNameResults[0].ToString() : "X-AnchorMailbox";

                        Headers[routingHintHeaderName] = routingHint;
                    }
                }
                else
                {
                    Headers["X-AnchorMailbox"] = "UPN:" + AuthInfo.Properties["UserPrincipalName"].Value;
                }
            }

            var connectionContextObjectId = SessionState.PSVariable.GetValue("ConnectionContextObjectId");
            if (connectionContextObjectId != null)
            {
                Headers["connection-id"] = (SessionState.PSVariable.GetValue("ConnectionContext") as PSObject)?.Properties["ConnectionId"].Value;
            }

            Headers["X-ResponseFormat"] = "clixml";
            Headers["X-SerializationLevel"] = "Partial";

            var psuiculture = SessionState.PSVariable.GetValue("PSUICulture") as string;
            if (!string.IsNullOrEmpty(psuiculture))
            {
                Headers["Accept-Language"] = psuiculture;
            }

            Headers["Accept-Encoding"] = "gzip";

            var getModuleVersion = InvokeCommand.GetCommand("Get-ModuleVersion", CommandTypes.Cmdlet) as PSCmdlet;
            if (getModuleVersion != null)
            {
                var versionResults = new PSDataCollection<PSObject>();
                getModuleVersion.Invoke<PSObject, PSObject>(null, versionResults);
                Headers["X-ClientModuleVersion"] = versionResults.Count > 0 ? versionResults[0].ToString() : null;
            }

            Headers["X-ClientApplication"] = "ExoManagementModule";

            WriteObject(Headers);
        }
    }

    [Cmdlet(VerbsCommon.Get, "HttpTimeout")]
    public class GetHttpTimeout : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Hashtable Parameters { get; set; }

        protected override void ProcessRecord()
        {
            var cmdletHttpTimeoutCollection = SessionState.PSVariable.GetValue("CmdletHttpTimeoutCollection") as Hashtable;
            var defaultHttpTimeout = (int)SessionState.PSVariable.GetValue("DefaultHTTPTimeout");

            if (Parameters.ContainsKey("X-CmdletName") && cmdletHttpTimeoutCollection != null)
            {
                var cmdletName = Parameters["X-CmdletName"] as string;
                if (!string.IsNullOrEmpty(cmdletName) && cmdletHttpTimeoutCollection.ContainsKey(cmdletName))
                {
                    WriteObject((int)cmdletHttpTimeoutCollection[cmdletName] + 60);
                    return;
                }
            }

            WriteObject(defaultHttpTimeout);
        }
    }

    [Cmdlet(VerbsCommon.Get, "ResultSize")]
    public class GetResultSize : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public Hashtable Parameters { get; set; }

        protected override void ProcessRecord()
        {
            var defaultPageSize = (int)SessionState.PSVariable.GetValue("DefaultPageSize");

            if (Parameters.ContainsKey("ResultSize"))
            {
                var resultSize = Parameters["ResultSize"];
                if (resultSize is string resultSizeString && resultSizeString.Equals("Unlimited", StringComparison.OrdinalIgnoreCase))
                {
                    WriteObject("Unlimited");
                }
                else if (int.TryParse(resultSize.ToString(), out int resultSizeInt))
                {
                    if (resultSizeInt <= 0)
                    {
                        WriteError(new ErrorRecord(new ArgumentException("ResultSize must be greater than 0"), "InvalidResultSize", ErrorCategory.InvalidArgument, null));
                        return;
                    }
                    WriteObject(resultSizeInt);
                }
                else
                {
                    WriteError(new ErrorRecord(new ArgumentException("Invalid ResultSize value"), "InvalidResultSize", ErrorCategory.InvalidArgument, null));
                }
            }
            else
            {
                WriteObject(defaultPageSize);
            }
        }
    }
}

public class ResultHandlingCmdlets
{
    //* This function prints the Result after the cmdlet executed successfully
    //* It additionally checks if the another Page needs to be fetched.
    //* This function is called at 4 places in the script, out of 3 are batching cases.
    //* In cases of batching, we want to print confirmation message as well, which is why
    //* we have passed isFromBatchingRequest = true by default. Non-batching case will pass this as false.
    [Cmdlet(VerbsCommon.Show, "ResultAndCheckForNextPage")]
    public class ShowResultAndCheckForNextPage : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public PSObject ResultObject { get; set; }

        [Parameter(Mandatory = true)]
        public bool VerifyNextPageLinkCheckRequired { get; set; }

        [Parameter(Mandatory = true)]
        public SwitchParameter AnotherPagedQuery { get; set; }

        [Parameter]
        public object ResultSize { get; set; }

        [Parameter]
        public int? NextPageSize { get; set; }

        [Parameter]
        public bool IsFromBatchingRequest { get; set; } = true;

        protected override void ProcessRecord()
        {
            var deserializedResult = PSSerializer.Deserialize(ResultObject.Properties["value"].Value as PSObject[]);
            WriteObject(deserializedResult);

            if (VerifyNextPageLinkCheckRequired)
            {
                var checkNextPageLinkExists = new CheckNextPageLinkExists
                {
                    ResultObject = ResultObject,
                    DeserializedResult = deserializedResult,
                    ResultSize = ResultSize,
                    NextPageSize = NextPageSize,
                    CommandRuntime = this.CommandRuntime
                };
                var results = new PSDataCollection<PSObject>();
                checkNextPageLinkExists.Invoke<PSObject, PSObject>(null, results);
                AnotherPagedQuery = results.Count > 0 && (bool)results[0].BaseObject;
            }

            var warnings = ResultObject.Properties["@adminapi.warnings"]?.Value as PSObject[];
            if (warnings != null)
            {
                var filterWarningsInPagination = SessionState.PSVariable.GetValue("FilterWarningsInPagination") as string;
                string filterWarningsInPaginationDecoded = "";
                foreach (var warning in warnings)
                {
                    if (string.IsNullOrEmpty(filterWarningsInPaginationDecoded))
                    {
                        filterWarningsInPaginationDecoded = Encoding.UTF8.GetString(Convert.FromBase64String(filterWarningsInPagination));
                    }

                    if (!AnotherPagedQuery || !filterWarningsInPaginationDecoded.Contains(warning.ToString()))
                    {
                        WriteWarning(warning.ToString());
                    }
                }
            }

            var errorsCaptured = ResultObject.Properties["@adminapi.errors"]?.Value as PSObject[];
            if (errorsCaptured != null)
            {
                foreach (var exc in errorsCaptured)
                {
                    SessionState.PSVariable.Set("EXO_LastExecutionStatus", false);
                    WriteError(new ErrorRecord(new Exception(exc.ToString()), "AdminApiError", ErrorCategory.NotSpecified, null));
                }
            }

            if (IsFromBatchingRequest)
            {
                var confirmationMessage = ResultObject.Properties["@adminapi.confirmationmessage"]?.Value as string;
                if (!string.IsNullOrEmpty(confirmationMessage))
                {
                    Host.UI.WriteLine($"What if: {confirmationMessage}");
                }
            }
        }
    }

    [Cmdlet(VerbsLifecycle.Confirm, "NextPageLinkExists")]
    public class CheckNextPageLinkExists : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public PSObject ResultObject { get; set; }

        [Parameter(Mandatory = true)]
        public PSObject DeserializedResult { get; set; }

        [Parameter]
        public object ResultSize { get; set; }

        [Parameter]
        public int? NextPageSize { get; set; }

        protected override void ProcessRecord()
        {
            if (!(ResultSize is string resultSizeString) || !resultSizeString.Equals("Unlimited", StringComparison.OrdinalIgnoreCase))
            {
                if (ResultSize is int resultSizeInt && NextPageSize.HasValue)
                {
                    int receivedSize = ((PSObject[])DeserializedResult).Length;
                    if (receivedSize == NextPageSize && resultSizeInt > NextPageSize && ResultObject.Properties["@odata.nextLink"]?.Value != null)
                    {
                        WriteObject(true);
                        return;
                    }
                }
            }
            else if (ResultObject.Properties["@odata.nextLink"]?.Value != null)
            {
                WriteObject(true);
                return;
            }

            WriteObject(false);
        }
    }

    [Cmdlet(VerbsLifecycle.Confirm, "RetryAndHandleWaitTime")]
    public class CheckRetryAndHandleWaitTime : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public int RetryCount { get; set; }

        [Parameter(Mandatory = true)]
        public ErrorRecord ErrorObject { get; set; }

        [Parameter(Mandatory = true)]
        public string CmdletRequestId { get; set; }

        [Parameter]
        public bool IsFromBatchingRequest { get; set; } = true;

        protected override void ProcessRecord()
        {
            var globalError = SessionState.PSVariable.Get("Error").Value as ArrayList;
            if (globalError != null && globalError.Count > 0)
            {
                var firstError = globalError[0] as ErrorRecord;
                if (firstError?.CategoryInfo?.Activity == "Invoke-WebRequest")
                {
                    globalError.RemoveAt(0);
                }
            }

            WriteVerbose($"Query {RetryCount + 1} failed.");

            var defaultMaxRetryTimes = (int)SessionState.PSVariable.GetValue("DefaultMaxRetryTimes");
            if (RetryCount == defaultMaxRetryTimes)
            {
                LogColumn("GenericError", CmdletRequestId, "Maximum error count reached:");
                LogColumn("GenericError", CmdletRequestId, ErrorObject.ToString());
                WriteError(new ErrorRecord(new Exception("Maximum retry count reached."), "MaxRetryReached", ErrorCategory.LimitExceeded, null));
                WriteErrorMessage(ErrorObject);
                WriteObject(false);
                return;
            }

            var getWaitTimeForRetry = new GetWaitTimeForRetry
            {
                ErrorObject = ErrorObject,
                RetryCount = RetryCount,
                CommandRuntime = this.CommandRuntime
            };
            var results = new PSDataCollection<PSObject>();
            getWaitTimeForRetry.Invoke<PSObject, PSObject>(null, results);
            int waitTime = results.Count > 0 ? (int)results[0].BaseObject : -1;

            if (waitTime >= 0)
            {
                WriteVerbose($"Sleeping for {waitTime} milliseconds");
                System.Threading.Thread.Sleep(waitTime);
                WriteObject(true);
            }
            else
            {
                LogColumn("GenericError", CmdletRequestId, ErrorObject.ToString());
                WriteErrorMessage(ErrorObject);
                WriteObject(false);
            }
        }

        private void LogColumn(string columnName, string cmdletId, string value)
        {
            var logColumn = InvokeCommand.GetCommand("Log-Column", CommandTypes.Cmdlet) as PSCmdlet;
            if (logColumn != null)
            {
                logColumn.Parameters.Add("ColumnName", columnName);
                logColumn.Parameters.Add("CmdletId", cmdletId);
                logColumn.Parameters.Add("Value", value);
                logColumn.Invoke();
            }
        }

        private void WriteErrorMessage(ErrorRecord errorObject)
        {
            var writeErrorMessage = InvokeCommand.GetCommand("Write-ErrorMessage", CommandTypes.Cmdlet) as PSCmdlet;
            if (writeErrorMessage != null)
            {
                writeErrorMessage.Parameters.Add("ErrorObject", errorObject);
                writeErrorMessage.Invoke();
            }
        }
    }

    [Cmdlet(VerbsCommon.Get, "WaitTimeForRetry")]
    public class GetWaitTimeForRetry : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public ErrorRecord ErrorObject { get; set; }

        [Parameter(Mandatory = true)]
        public int RetryCount { get; set; }

        protected override void ProcessRecord()
        {
            var retryableExceptions = SessionState.PSVariable.GetValue("RetryableExceptions") as string[];
            var retryableHTTPResponses = SessionState.PSVariable.GetValue("RetryableHTTPResponses") as int[];

            if ((retryableExceptions?.Length ?? 0) == 0 && (retryableHTTPResponses?.Length ?? 0) == 0)
            {
                WriteVerbose("Permanent Exception occurred: this request will not be retried.");
                WriteObject(-1);
                return;
            }

            if (ErrorObject?.Exception?.Response == null || ErrorObject.ErrorDetails == null)
            {
                WriteVerbose("Unknown Exception occurred: this request will not be retried.");
                WriteObject(-1);
                return;
            }

            var errorStatusCode = (int)(ErrorObject.Exception.Response as System.Net.HttpWebResponse)?.StatusCode;
            var errorDetails = JObject.Parse(ErrorObject.ErrorDetails.Message);
            var errorDetailsMessage = errorDetails["error"]?["details"]?[0]?["message"]?.ToString() ?? "";

            int waitTimeForRetry = -1;

            if (retryableHTTPResponses?.Contains(errorStatusCode) ?? false)
            {
                waitTimeForRetry = 0;
                WriteVerbose($"Exception occurred: HTTP status code {errorStatusCode}; Request is being retried.");
            }

            if (errorStatusCode == 429)
            {
                var retryAfter = ErrorObject.Exception.Response.Headers["Retry-After"];
                if (string.IsNullOrEmpty(retryAfter))
                {
                    waitTimeForRetry = 0;
                    WriteVerbose("Request has been throttled, but the response header does not contain any retry wait time.");
                }
                else
                {
                    waitTimeForRetry = (int)(double.Parse(retryAfter) * 1000);
                    WriteVerbose($"The current request is throttled at the server, waiting for {waitTimeForRetry} milliseconds");
                }
            }

            if (waitTimeForRetry == -1)
            {
                foreach (var exceptionName in retryableExceptions ?? Array.Empty<string>())
                {
                    if (errorDetailsMessage.Contains(exceptionName))
                    {
                        waitTimeForRetry = 0;
                        WriteVerbose($"Exception occurred: Exception {exceptionName}; Request is being retried.");
                        break;
                    }
                }
            }

            if (waitTimeForRetry == 0)
            {
                var defaultRetryInterval = (int)SessionState.PSVariable.GetValue("DefaultRetryIntervalInMilliSeconds");
                var defaultRetryIntervalBase = (double)SessionState.PSVariable.GetValue("DefaultRetryIntervalExponentBase");
                waitTimeForRetry = (int)(defaultRetryInterval * Math.Pow(defaultRetryIntervalBase, RetryCount));
                WriteVerbose("Using exponential backoff retry logic to get the wait time for next retry");
            }

            WriteObject(waitTimeForRetry);
        }
    }
}

//* #################################  README  ###################################
//TODO: Transform-Parameters and Execute-Command are incredibly complex. Need to set aside a bit more time for manual review of these but for now I am just going tood an empty declaration.
//* ##############################################################################

[Cmdlet(VerbsCommon.Transform, "Parameters")]
public class TransformParameters : PSCmdlet
{ }

[Cmdlet(VerbsCommon.Execute, "Command")]
public class ExecuteCommand : PSCmdlet
{ }

[Cmdlet(VerbsCommon.AddTo, "BatchRequest")]
public class AddToBatchRequest
{}

[Cmdlet(VerbsCommon.Execute, "BatchRequest")]
public class ExecuteBatchRequest
{}

[Cmdlet(VerbsCommon.Execute, "BatchedNextPageRequest")]
public class ExecuteBatchedNextPageRequest
{}

[Cmdlet(VerbsCommon.Retry, "SingleBatchedRequest")]
public class RetrySingleBatchedRequest
{}

[Cmdlet(VerbsCommon.Deserialize, "BatchRequest")]
public class DeserializeBatchRequest
{}

[Cmdlet(VerbsCommon.Create, "CustomError")]
public class CreateCustomError
{}

[Cmdlet(VerbsCommon.Init, "CmdletProcess")]
public class InitCmdletProcess
{}

[Cmdlet(VerbsCommon.Commit, "CmdletLogOnError")]
public class CommitCmdletLogOnError
{}

[Cmdlet(VerbsCommon.Execute, "CmdletEndBlock")]
public class ExecuteCmdletEndBlock
{}

[Cmdlet(VerbsCommon.Execute, "CmdletBatchingBeginBody")]
public class ExecuteCmdletBatchingBeginBody
{}

[Cmdlet(VerbsCommon.Log, "EndTimeInCmdletProcessBlock")]
public class LogEndTimeInCmdletProcessBlock
{}
// endregion