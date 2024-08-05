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
using System.Threading.Tasks;

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

[Cmdlet(VerbsCommon.Transform, "Parameters")]
public class TransformParametersCmdlet : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public Hashtable Parameters { get; set; }

    [Parameter(Mandatory = true)]
    public Type CmdletClass { get; set; }

    protected override void ProcessRecord()
    {
        var result = TransformParameters(Parameters, CmdletClass);
        WriteObject(result);
    }

    private Dictionary<string, object> TransformParameters(Hashtable parameters, Type cmdletClass)
    {
        var result = new Dictionary<string, object>();

        foreach (DictionaryEntry param in parameters)
        {
            string key = param.Key.ToString();
            object value = param.Value;

            if (value == null)
            {
                result[key] = null;
                continue;
            }

            if (value is SwitchParameter switchParam)
            {
                result[key] = switchParam.ToBool();
            }
            else if (value is Hashtable hashtable)
            {
                result[key] = ConvertToHashtable(hashtable);
            }
            else if (PowerShellParameters.Contains(key))
            {
                result[key] = value.ToString();
            }
            else if (value is Hashtable[] hashtableArray)
            {
                result[key + "@odata.type"] = "#Collection(Exchange.GenericHashTable)";
                result[key] = hashtableArray;
            }
            else if (cmdletClass.GetProperty(key)?.PropertyType.Name == "PSObject" ||
                     cmdletClass.GetProperty(key)?.PropertyType.Name == "PSCustomObject")
            {
                result[key] = value;
            }
            else if (value is PSObject psObject)
            {
                var cmdletParam = cmdletClass.GetProperty(key);
                if (cmdletParam != null &&
                    (cmdletParam.GetCustomAttributes(typeof(ParameterAttribute), false)
                        .Cast<ParameterAttribute>()
                        .Any(attr => attr.ValueFromPipeline || attr.ValueFromPipelineByPropertyName) &&
                     psObject.Properties[key] != null))
                {
                    result[key] = psObject.Properties[key].Value.ToString();
                }
                else
                {
                    result[key] = psObject.ToString();
                }
            }
            else if (value is object[] objArray && objArray.Length > 0 && objArray[0] is PSObject)
            {
                result[key] = objArray.Select(obj => obj.ToString()).ToArray();
            }
            else if (value is byte[] byteArray)
            {
                result[key] = ConvertToByteArray(byteArray);
            }
            else if ((value is object[] || value is ArrayList) &&
                     ((IList)value).Count > 0 && ((IList)value)[0] is byte[])
            {
                result[key + "@odata.type"] = "#Collection(Exchange.ByteArrayType)";
                result[key] = ConvertToByteArrayCollection((IList)value);
            }
            else if (value is DateTime dateTime)
            {
                result[key] = dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
            }
            else if (value is DateTimeOffset dateTimeOffset)
            {
                result[key] = dateTimeOffset.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
            }
            else if (value is IPAddress ipAddress)
            {
                result[key] = ipAddress.ToString();
            }
            else if (value is CultureInfo cultureInfo)
            {
                result[key] = cultureInfo.ToString();
            }
            else if (value is TimeSpan timeSpan)
            {
                result[key] = timeSpan.ToString();
            }
            else if (value.GetType().BaseType == typeof(string))
            {
                result[key] = value.ToString();
            }
            else if (value is object[] stringArray && stringArray.Length > 0 && stringArray[0] is string)
            {
                result[key] = stringArray.Cast<string>().ToArray();
            }
            else if (value is ScriptBlock || (value is object[] scriptBlockArray && scriptBlockArray[0] is ScriptBlock))
            {
                result[key] = value.ToString();
            }
            else
            {
                result[key] = value;
            }

            if (FilterParametersForBooleanParsing.Contains(key))
            {
                result[key] = SanitizeBoolValuesInFilter(value);
            }
        }

        return result;
    }

    private static readonly HashSet<string> PowerShellParameters = new HashSet<string>
    { };

    private readonly HashSet<string> FilterParametersForBooleanParsing = new HashSet<string>

    private object SanitizeBoolValuesInFilter(object value)
    {
        var boolValues = new[] { "true", "false" };
        if (FilterParametersForBooleanParsing.Contains(value.ToString().Trim()) && boolValues.Contains(value.ToString()))
        {
            return value.ToString().Trim().ToLower() == "true";
        }
        return value;
    }

    private object ConvertToByteArrayCollection(IList value)
    {
        return value.Cast<byte[]>().Select(ConvertToByteArray).ToArray();
    }

    private object ConvertToByteArray(byte[] byteArray)
    {
        return byteArray.ToArray();
    }

    private object ConvertToHashtable(Hashtable hashtable)
    {
        var result = new Dictionary<string, object>();
        foreach (DictionaryEntry entry in hashtable)
        {
            result.Add(entry.Key.ToString(), entry.Value);
        }
        return result;
    }
}

[Cmdlet(VerbsCommon.Execute, "Command")]
public class ExecuteCommand : PSCmdlet
{
    private const int DefaultPageSize = 1000;
    private const int DefaultMaxRetryTimes = 3; // Assuming this value, adjust as needed
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task ExecuteCommandAsync(
        string cmdletName,
        Dictionary<string, object> parameters,
        Func<string, string> parameterBasedRoutingHintDelegate,
        string parameterBasedRoutingHintParameterValue,
        bool showConfirmPrompt = false,
        PSCmdlet psCmdletObject = null,
        bool useParameterBasedRouting = false,
        string cmdletRequestId = null)
    {
        int resultSize = DefaultPageSize;
        int nextPageSize = DefaultPageSize;
        bool anotherPagedQuery = true;

        var headers = new Dictionary<string, string>
        {
            ["Accept"] = "application/json",
            ["Accept-Charset"] = "UTF-8",
            ["Content-Type"] = "application/json",
            ["X-CmdletName"] = cmdletName,
            ["client-request-id"] = cmdletRequestId ?? Guid.NewGuid().ToString()
        };

        if (parameters.TryGetValue("WarningAction", out var warningAction))
        {
            headers["WarningAction"] = warningAction.ToString() == "Ignore" ? "SilentlyContinue" : warningAction.ToString();
        }

        parameters.Remove("UseCustomRouting");

        var cmdletClass = typeof(PSCmdlet).Assembly.GetType($"System.Management.Automation.Cmdlets.{cmdletName}Cmdlet");
        var transformedParams = TransformParameters(parameters, cmdletClass);

        if (showConfirmPrompt && !transformedParams.ContainsKey("Confirm"))
        {
            transformedParams["Confirm"] = true;
        }

        var cmdletInput = new Dictionary<string, object>
        {
            ["CmdletName"] = cmdletName,
            ["Parameters"] = transformedParams
        };

        var bodyObj = new Dictionary<string, object>
        {
            ["CmdletInput"] = cmdletInput
        };

        resultSize = GetResultSize(parameters);
        if (resultSize == -1) return;

        if (resultSize != "Unlimited")
        {
            nextPageSize = Math.Min(resultSize, DefaultPageSize);
        }

        string nextPageUri = null;

        while (anotherPagedQuery)
        {
            headers["Prefer"] = $"odata.maxpagesize={nextPageSize}";

            bool isRetryHappening = true;
            string claims = string.Empty;

            for (int retryCount = 0; retryCount <= DefaultMaxRetryTimes && isRetryHappening; retryCount++)
            {
                try
                {
                    // Authentication logic here (replace with your actual auth implementation)
                    var authInfo = GetAuthInfo(claims, cmdletRequestId);
                    claims = string.Empty;

                    var startTime = DateTime.Now;
                    InitLog(cmdletRequestId);
                    LogColumn("StartTime", cmdletRequestId, startTime);
                    LogColumn("CmdletName", cmdletRequestId, cmdletName);
                    LogColumn("CmdletParameters", cmdletRequestId, parameters);

                    var uri = string.IsNullOrWhiteSpace(nextPageUri) 
                        ? $"{authInfo.BaseUri}/{authInfo.TenantId}/InvokeCommand" 
                        : nextPageUri;

                    PopulateHeaders(parameterBasedRoutingHintDelegate, parameterBasedRoutingHintParameterValue, 
                        useParameterBasedRouting, authInfo, headers);

                    if (showConfirmPrompt)
                    {
                        Console.WriteLine("Validation call to the server:");
                    }

                    var timeout = GetHttpTimeout(headers);
                    var content = new StringContent(JsonConvert.SerializeObject(bodyObj), Encoding.UTF8, "application/json");
                    
                    var response = await httpClient.PostAsync(uri, content);
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    var resultObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    var confirmationMessage = resultObject.ContainsKey("@adminapi.confirmationmessage") 
                        ? resultObject["@adminapi.confirmationmessage"].ToString() 
                        : string.Empty;

                    if (psCmdletObject?.ShouldProcess(confirmationMessage) == true && showConfirmPrompt)
                    {
                        transformedParams["Confirm"] = false;
                        cmdletInput["Parameters"] = transformedParams;
                        bodyObj["CmdletInput"] = cmdletInput;

                        content = new StringContent(JsonConvert.SerializeObject(bodyObj), Encoding.UTF8, "application/json");
                        Console.WriteLine("Execution call to the server:");
                        response = await httpClient.PostAsync(uri, content);
                        response.EnsureSuccessStatusCode();
                        responseContent = await response.Content.ReadAsStringAsync();
                        resultObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                    }

                    isRetryHappening = false;

                    var endTime = DateTime.Now;
                    LogColumn("EndTime", cmdletRequestId, endTime);
                    CommitLog(cmdletRequestId);

                    PrintResultAndCheckForNextPage(resultObject, true, ref anotherPagedQuery, resultSize, nextPageSize, false);
                    
                    if (anotherPagedQuery)
                    {
                        nextPageUri = resultObject["@odata.nextLink"].ToString();
                        if (resultSize != "Unlimited")
                        {
                            resultSize -= nextPageSize;
                            nextPageSize = Math.Min(resultSize, DefaultPageSize);
                        }
                    }
                }
                catch (Exception ex)
                {
                    claims = GetClaimsFromExceptionDetails(ex);
                    bool isRetryable = CheckRetryAndHandleWaitTime(retryCount, ex, cmdletRequestId, false);
                    if (!isRetryable)
                    {
                        // Set global execution status (you may want to handle this differently in C#)
                        // $global:EXO_LastExecutionStatus = false;
                        return;
                    }
                    else
                    {
                        LogColumn("GenericError", cmdletRequestId, ex);
                        var endTime = DateTime.Now;
                        LogColumn("EndTime", cmdletRequestId, endTime);
                        CommitLog(cmdletRequestId);
                    }
                }
            }

            if (string.IsNullOrEmpty(nextPageUri))
            {
                anotherPagedQuery = false;
            }
        }
    }
}
public class AuthInfo
{
    public string BaseUri { get; set; }
    public string TenantId { get; set; }
    // Add other properties as needed
}

[Cmdlet(VerbsCommon.AddTo, "BatchRequest")]
public class AddToBatchRequest : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public object BodyObj { get; set; }

    [Parameter]
    public object ParameterBasedRoutingHintDelegate { get; set; }

    [Parameter]
    public string ParameterBasedRoutingHintParameterValue { get; set; }

    [Parameter]
    public bool UseParameterBasedRouting { get; set; } = false;

    [Parameter]
    public string CmdletRequestId { get; set; }

    private const int DefaultPageSize = 1000; // Assuming this value, adjust as needed

    protected override void ProcessRecord()
    {
        var resultSize = GetResultSize(MyInvocation.BoundParameters);
        int? nextPageSize = null;
        if (resultSize != "Unlimited" && int.TryParse(resultSize, out int size) && size > 0)
        {
            nextPageSize = Math.Min(size, DefaultPageSize);
        }

        string claims = string.Empty;
        AuthInfo authInfo;

        if (SessionState.PSVariable.GetValue("ConnectionContextObjectId") is string connectionContextObjectId && !string.IsNullOrEmpty(connectionContextObjectId))
        {
            var connectionContext = ConnectionContextFactory.GetCurrentConnectionContext(connectionContextObjectId);
            authInfo = connectionContext.GetAuthHeader(claims, CmdletRequestId);
            claims = string.Empty;
        }
        else
        {
            // Assuming GetAuthInfoUsingTokenProvider is implemented elsewhere
            var tokenProviderObjectId = SessionState.PSVariable.GetValue("TokenProviderObjectId") as string;
            authInfo = GetAuthInfoUsingTokenProvider(tokenProviderObjectId);
        }

        string uri = $"{authInfo.BaseUri}/{authInfo.TenantId}/InvokeCommand";

        var headers = new Dictionary<string, string>();
        PopulateHeaders(ParameterBasedRoutingHintDelegate, ParameterBasedRoutingHintParameterValue, UseParameterBasedRouting, authInfo, headers);

        if (nextPageSize.HasValue)
        {
            headers["Prefer"] = $"odata.maxpagesize={nextPageSize};odata.continue-on-error";
        }

        WriteVerbose($"Adding {GetBatchRequestCount() + 1} Request to Batch");

        var batchRequest = new Dictionary<string, object>
        {
            ["url"] = uri,
            ["method"] = "POST",
            ["body"] = BodyObj,
            ["headers"] = headers,
            ["id"] = GetBatchRequestCount().ToString()
        };

        AddToBatchBodyObj(batchRequest);
        AddToBatchRequestParameters(MyInvocation.BoundParameters);
    }
}

[Cmdlet(VerbsLifecycle.Execute, "BatchRequest")]
public class ExecuteBatchRequest : PSCmdlet
{
    [Parameter]
    public string CmdletRequestId { get; set; }

    private const int DefaultMaxRetryTimes = 3; // Adjust as needed
    private static readonly HttpClient httpClient = new HttpClient();

    protected override void ProcessRecord()
    {
        var batchBodyObj = GetBatchBodyObj();
        string batchBody = JsonConvert.SerializeObject(batchBodyObj, new JsonSerializerSettings { MaxDepth = 10 });
        byte[] batchBodyBytes = Encoding.UTF8.GetBytes(batchBody);

        var batchedRequests = (JArray)batchBodyObj["requests"];
        var lastBatchedRequest = (JObject)batchedRequests[batchedRequests.Count - 1];
        var batchHeaders = new Dictionary<string, string>(lastBatchedRequest["headers"].ToObject<Dictionary<string, string>>());
        batchHeaders["Prefer"] = "odata.continue-on-error";

        WriteVerbose("Executing Batched Request");

        bool isRetryable = true;
        string claims = string.Empty;

        for (int retryCount = 0; retryCount <= DefaultMaxRetryTimes && isRetryable; retryCount++)
        {
            AuthInfo authInfo;
            if (SessionState.PSVariable.GetValue("ConnectionContextObjectId") is string connectionContextObjectId && !string.IsNullOrEmpty(connectionContextObjectId))
            {
                var connectionContext = ConnectionContextFactory.GetCurrentConnectionContext(connectionContextObjectId);
                authInfo = connectionContext.GetAuthHeader(claims, CmdletRequestId);
                claims = string.Empty;
            }
            else
            {
                var tokenProviderObjectId = SessionState.PSVariable.GetValue("TokenProviderObjectId") as string;
                authInfo = GetAuthInfoUsingTokenProvider(tokenProviderObjectId);
            }

            batchHeaders["Authorization"] = authInfo.AuthorizationHeader;
            string batchUri = $"{authInfo.BaseUri}/{authInfo.TenantId}/$batch";

            try
            {
                int timeout = GetHttpTimeout(batchHeaders);
                var response = ExecuteHttpRequestAsync(batchUri, batchHeaders, batchBodyBytes, timeout).GetAwaiter().GetResult();
                WriteObject(response);
                return;
            }
            catch (Exception ex)
            {
                claims = GetClaimsFromExceptionDetails(ex);
                isRetryable = CheckRetryAndHandleWaitTime(retryCount, ex, CmdletRequestId);
            }
        }

        WriteObject(null);
    }
}

[Cmdlet(VerbsLifecycle.Execute, "BatchedNextPageRequest")]
public class ExecuteBatchedNextPageRequest : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public PSObject PagedRequest { get; set; }

    [Parameter(Mandatory = true)]
    public PSObject PagedResponse { get; set; }

    [Parameter(Mandatory = true)]
    public object ResultSize { get; set; }

    [Parameter(Mandatory = true)]
    public int NextPageSize { get; set; }

    [Parameter]
    public string CmdletRequestId { get; set; }

    private const int DefaultMaxRetryTimes = 3; // Adjust as needed
    private const int DefaultPageSize = 1000; // Adjust as needed
    private static readonly HttpClient httpClient = new HttpClient();

    protected override void ProcessRecord()
    {
        var prevPagedResponse = PagedResponse;
        bool anotherPagedQuery = true;

        while (anotherPagedQuery)
        {
            if (ResultSize.ToString() != "Unlimited")
            {
                int resultSizeInt = Convert.ToInt32(ResultSize);
                resultSizeInt -= NextPageSize;
                NextPageSize = Math.Min(resultSizeInt, DefaultPageSize);
                ResultSize = resultSizeInt;
            }

            var nextPageRequest = JObject.FromObject(PagedRequest);
            nextPageRequest["url"] = prevPagedResponse.Properties["@odata.nextLink"].Value.ToString();

            string nextPageRequestBody = JsonConvert.SerializeObject(nextPageRequest["body"], new JsonSerializerSettings { MaxDepth = 10 });
            byte[] nextPageRequestBodyBytes = Encoding.UTF8.GetBytes(nextPageRequestBody);

            var nextPageRequestHeaders = (JObject)nextPageRequest["headers"];
            nextPageRequestHeaders["Prefer"] = $"odata.maxpagesize={NextPageSize}";

            WriteVerbose($"Executing Batched NextPage Request");
            WriteVerbose($"NextPage URL : {nextPageRequest["url"]}");

            bool isRetryable = true;
            string claims = string.Empty;

            for (int retryCount = 0; retryCount <= DefaultMaxRetryTimes && isRetryable; retryCount++)
            {
                AuthInfo authInfo;
                if (SessionState.PSVariable.GetValue("ConnectionContextObjectId") is string connectionContextObjectId && !string.IsNullOrEmpty(connectionContextObjectId))
                {
                    var connectionContext = ConnectionContextFactory.GetCurrentConnectionContext(connectionContextObjectId);
                    authInfo = connectionContext.GetAuthHeader(claims, CmdletRequestId);
                    claims = string.Empty;
                }
                else
                {
                    var tokenProviderObjectId = SessionState.PSVariable.GetValue("TokenProviderObjectId") as string;
                    authInfo = GetAuthInfoUsingTokenProvider(tokenProviderObjectId);
                }

                nextPageRequestHeaders["Authorization"] = authInfo.AuthorizationHeader;

                try
                {
                    int timeout = GetHttpTimeout(nextPageRequestHeaders.ToObject<Dictionary<string, string>>());
                    var result = ExecuteHttpRequestAsync(nextPageRequest["url"].ToString(), nextPageRequestHeaders.ToObject<Dictionary<string, string>>(), nextPageRequestBodyBytes, timeout).GetAwaiter().GetResult();

                    if (!string.IsNullOrEmpty(result))
                    {
                        var resultObject = JsonConvert.DeserializeObject<JObject>(result);
                        PrintResultAndCheckForNextPage(resultObject, true, ref anotherPagedQuery, ResultSize, NextPageSize);
                        isRetryable = false;
                        prevPagedResponse = PSObject.AsPSObject(resultObject);
                    }
                }
                catch (Exception ex)
                {
                    claims = GetClaimsFromExceptionDetails(ex);
                    isRetryable = CheckRetryAndHandleWaitTime(retryCount, ex, CmdletRequestId);
                }
            }
        }
    }
}

[Cmdlet(VerbsDiagnostic.Retry, "SingleBatchedRequest")]
public class RetrySingleBatchedRequest : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public PSObject FailedRequest { get; set; }

    [Parameter(Mandatory = true)]
    public PSObject ErrorObject { get; set; }

    [Parameter]
    public string CmdletRequestId { get; set; }

    private const int DefaultMaxRetryTimes = 3; // Adjust as needed
    private const int DefaultPageSize = 1000; // Adjust as needed
    private static readonly HttpClient httpClient = new HttpClient();

    protected override void ProcessRecord()
    {
        string body = JsonConvert.SerializeObject(FailedRequest.Properties["body"].Value, new JsonSerializerSettings { MaxDepth = 10 });
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
        var headers = ((PSObject)FailedRequest.Properties["headers"].Value).Properties.ToDictionary(p => p.Name, p => p.Value.ToString());
        string uri = FailedRequest.Properties["url"].Value.ToString();

        bool isRetryHappening = true;
        bool isQuerySuccessful = false;
        string claims = GetClaimsFromExceptionDetails(ErrorObject);
        isRetryHappening = CheckRetryAndHandleWaitTime(0, ErrorObject, CmdletRequestId);

        for (int retryCount = 1; retryCount <= DefaultMaxRetryTimes && isRetryHappening; retryCount++)
        {
            AuthInfo authInfo;
            if (SessionState.PSVariable.GetValue("ConnectionContextObjectId") is string connectionContextObjectId && !string.IsNullOrEmpty(connectionContextObjectId))
            {
                var connectionContext = ConnectionContextFactory.GetCurrentConnectionContext(connectionContextObjectId);
                authInfo = connectionContext.GetAuthHeader(claims, CmdletRequestId);
                claims = string.Empty;
            }
            else
            {
                var tokenProviderObjectId = SessionState.PSVariable.GetValue("TokenProviderObjectId") as string;
                authInfo = GetAuthInfoUsingTokenProvider(tokenProviderObjectId);
            }

            headers["Authorization"] = authInfo.AuthorizationHeader;

            try
            {
                var existingProgressPreference = SessionState.PSVariable.GetValue("ProgressPreference");
                SessionState.PSVariable.Set("ProgressPreference", "SilentlyContinue");

                int timeout = GetHttpTimeout(headers);
                var result = ExecuteHttpRequestAsync(uri, headers, bodyBytes, timeout).GetAwaiter().GetResult();

                SessionState.PSVariable.Set("ProgressPreference", existingProgressPreference);

                if (!string.IsNullOrEmpty(result))
                {
                    var resultObject = JsonConvert.DeserializeObject<JObject>(result);

                    int requestId = int.Parse(resultObject["id"].ToString());
                    var currentRequestParameters = GetBatchRequestParameters()[requestId];

                    var resultSize = GetResultSize(currentRequestParameters);
                    int nextPageSize = resultSize != "Unlimited" && int.TryParse(resultSize, out int size) && size > 0
                        ? Math.Min(size, DefaultPageSize)
                        : DefaultPageSize;

                    bool anotherPagedQuery = false;
                    PrintResultAndCheckForNextPage(resultObject, true, ref anotherPagedQuery, resultSize, nextPageSize);

                    if (anotherPagedQuery)
                    {
                        var pagedRequest = ((JArray)GetBatchBodyObj()["requests"])[requestId];
                        ExecuteBatchedNextPageRequest(pagedRequest, resultObject, resultSize, nextPageSize, CmdletRequestId);
                    }
                }

                isQuerySuccessful = true;
                isRetryHappening = false;
            }
            catch (Exception ex)
            {
                SessionState.PSVariable.Set("ProgressPreference", existingProgressPreference);
                claims = GetClaimsFromExceptionDetails(ex);
                isRetryHappening = CheckRetryAndHandleWaitTime(retryCount, ex, CmdletRequestId);
            }
        }

        if (!isQuerySuccessful)
        {
            SessionState.PSVariable.Set("EXO_LastExecutionStatus", false);
        }
    }
}

[Cmdlet(VerbsData.Deserialize, "BatchRequest")]
public class DeserializeBatchRequest : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public PSObject BatchResponse { get; set; }

    [Parameter]
    public string CmdletRequestId { get; set; }

    private const int DefaultPageSize = 1000; // Adjust as needed

    protected override void ProcessRecord()
    {
        int resultSize = DefaultPageSize;
        int nextPageSize = DefaultPageSize;
        bool anotherPagedQuery = true;

        if (BatchResponse == null || BatchResponse.Properties["Content"].Value == null)
        {
            return;
        }

        var batchBodyObj = GetBatchBodyObj();
        string batchBody = JsonConvert.SerializeObject(batchBodyObj, new JsonSerializerSettings { MaxDepth = 10 });
        var batchBodyPSObject = JsonConvert.DeserializeObject<dynamic>(batchBody);
        int batchBodyIterator = 0;

        var batchedResultObject = JsonConvert.DeserializeObject<JObject>(BatchResponse.Properties["Content"].Value.ToString());
        var responses = batchedResultObject["responses"] as JArray;

        foreach (var result in responses)
        {
            var headers = result["headers"] as JObject;
            headers.Add("Cmdlet-Name", batchBodyPSObject.requests[batchBodyIterator++].body.CmdletInput.CmdletName);

            if ((int)result["status"] == 200)
            {
                var resultObject = result["body"] as JObject;
                int requestId = int.Parse(result["id"].ToString());
                var currentRequestParameters = GetBatchRequestParameters()[requestId];

                resultSize = GetResultSize(currentRequestParameters);
                if (resultSize != -1 && resultSize > 0) // Assuming -1 represents "Unlimited"
                {
                    nextPageSize = Math.Min(resultSize, DefaultPageSize);
                }

                PrintResultAndCheckForNextPage(resultObject, true, ref anotherPagedQuery, resultSize, nextPageSize);

                if (anotherPagedQuery)
                {
                    var pagedRequest = ((JArray)batchBodyObj["requests"])[requestId];
                    ExecuteBatchedNextPageRequest(pagedRequest, resultObject, resultSize, nextPageSize, CmdletRequestId);
                }
            }
            else
            {
                // Retry individual request
                int requestId = int.Parse(result["id"].ToString());
                var failedRequest = ((JArray)batchBodyObj["requests"])[requestId];
                var customException = CreateCustomError(result, GetBatchResponseHeaders());
                RetrySingleBatchedRequest(failedRequest, customException, CmdletRequestId);
            }
        }
    }
}

[Cmdlet(VerbsCommon.New, "CustomError")]
public class CreateCustomError : PSCmdlet
{
    [Parameter(Mandatory = true, AllowNull = true)]
    public PSObject ErrorResult { get; set; }

    [Parameter(Mandatory = true, AllowNull = true)]
    public PSObject BatchedResponseHeaders { get; set; }

    protected override void ProcessRecord()
    {
        var customError = new PSObject();
        var exception = new PSObject();
        var response = new PSObject();
        var errorDetails = new PSObject();

        string jsonErrorMessage;
        if (ErrorResult == null)
        {
            jsonErrorMessage = @"{""error"": {""details"": [{""message"": ""Response is Null.""}]}}";
        }
        else
        {
            jsonErrorMessage = JsonConvert.SerializeObject(ErrorResult.Properties["body"].Value, new JsonSerializerSettings { MaxDepth = 10 });
        }

        if (ErrorResult != null && 
            ErrorResult.Properties["body"].Value != null && 
            ((dynamic)ErrorResult.Properties["body"].Value).error != null && 
            ((dynamic)ErrorResult.Properties["body"].Value).error.code != null)
        {
            response.Properties.Add(new PSNoteProperty("StatusCode", ((dynamic)ErrorResult.Properties["body"].Value).error.code));
        }

        if (BatchedResponseHeaders != null)
        {
            response.Properties.Add(new PSNoteProperty("Headers", BatchedResponseHeaders));
        }

        if (ErrorResult != null && 
            ErrorResult.Properties["headers"].Value != null && 
            response.Properties["Headers"] != null)
        {
            var responseHeaders = (PSObject)response.Properties["Headers"].Value;
            bool isValueArray = false;

            if (responseHeaders.Properties.Count >= 1)
            {
                var firstValue = responseHeaders.Properties[0].Value;
                isValueArray = firstValue is Array;
            }

            var errorResultHeaders = (PSObject)ErrorResult.Properties["headers"].Value;
            foreach (var property in errorResultHeaders.Properties)
            {
                if (isValueArray)
                {
                    responseHeaders.Properties.Add(new PSNoteProperty(property.Name, new[] { property.Value.ToString() }));
                }
                else
                {
                    responseHeaders.Properties.Add(new PSNoteProperty(property.Name, property.Value));
                }
            }
        }

        errorDetails.Properties.Add(new PSNoteProperty("Message", jsonErrorMessage));
        exception.Properties.Add(new PSNoteProperty("Response", response));
        customError.Properties.Add(new PSNoteProperty("Exception", exception));
        customError.Properties.Add(new PSNoteProperty("ErrorDetails", errorDetails));

        customError.Methods.Add(new PSScriptMethod("ToString", () => ((PSObject)customError.Properties["ErrorDetails"].Value).Properties["Message"].Value.ToString()));

        WriteObject(customError);
    }
}

[Cmdlet(VerbsLifecycle.Init, "CmdletBegin")]
public class InitCmdletBegin : PSCmdlet
{
    [Parameter(Mandatory = true, AllowNull = true)]
    public PSObject CmdletParameters { get; set; }

    [Parameter(Mandatory = true)]
    public string CmdletName { get; set; }

    protected override void ProcessRecord()
    {
        string cmdletRequestId = Guid.NewGuid().ToString();
        bool cmdletRequestIdGeneratedInBegin = true;

        // Initialize the cmdlet activity
        var cmdletIDList = new List<string> { cmdletRequestId };

        DateTime startTime = DateTime.Now;

        InitLog(cmdletRequestId);
        LogColumn("StartTime", cmdletRequestId, startTime);
        LogColumn("CmdletName", cmdletRequestId, MyInvocation.MyCommand.Name);
        LogColumn("CmdletParameters", cmdletRequestId, PSSerializer.Serialize(MyInvocation.BoundParameters));

        // Set global execution status
        SessionState.PSVariable.Set("EXO_LastExecutionStatus", true);

        var result = new PSObject();
        result.Properties.Add(new PSNoteProperty("CmdletRequestId", cmdletRequestId));
        result.Properties.Add(new PSNoteProperty("CmdletRequestIdGeneratedInBegin", cmdletRequestIdGeneratedInBegin));
        result.Properties.Add(new PSNoteProperty("CmdletIDList", cmdletIDList));

        WriteObject(result);
    }
}

[Cmdlet(VerbsLifecycle.Initialize, "CmdletProcess")]
public class InitCmdletProcess : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public bool CmdletRequestIdGeneratedInBegin { get; set; }

    [Parameter(Mandatory = true)]
    public bool UseBatching { get; set; }

    [Parameter(Mandatory = true)]
    public JObject BatchBodyObj { get; set; }

    [Parameter(Mandatory = true)]
    public string CmdletRequestId { get; set; }

    [Parameter(Mandatory = true)]
    public List<string> CmdletIDList { get; set; }

    [Parameter(Mandatory = true, AllowNull = true)]
    public PSObject CmdletParameters { get; set; }

    [Parameter(Mandatory = true)]
    public string CmdletName { get; set; }

    protected override void ProcessRecord()
    {
        bool cmdletRequestIdGeneratedInProcess = true;

        if (!CmdletRequestIdGeneratedInBegin && 
            (!UseBatching || (UseBatching && BatchBodyObj != null && ((JArray)BatchBodyObj["requests"]).Count == 0)))
        {
            // Initialize the cmdlet activity, this is pipeline request without batching, or it is the first request in a batch
            CmdletRequestId = Guid.NewGuid().ToString();
            cmdletRequestIdGeneratedInProcess = true;
            InitLog(CmdletRequestId);
            DateTime startTime = DateTime.Now;
            LogColumn("StartTime", CmdletRequestId, startTime);
            CmdletIDList.Add(CmdletRequestId);
        }

        CmdletRequestIdGeneratedInBegin = false;
        LogColumn("CmdletName", CmdletRequestId, CmdletName);
        LogColumn("CmdletParameters", CmdletRequestId, PSSerializer.Serialize(CmdletParameters));

        var result = new PSObject();
        result.Properties.Add(new PSNoteProperty("CmdletRequestIdGeneratedInBegin", CmdletRequestIdGeneratedInBegin));
        result.Properties.Add(new PSNoteProperty("CmdletRequestIdGeneratedInProcess", cmdletRequestIdGeneratedInProcess));
        result.Properties.Add(new PSNoteProperty("CmdletRequestId", CmdletRequestId));
        result.Properties.Add(new PSNoteProperty("CmdletIDList", CmdletIDList));

        WriteObject(result);
    }
}

[Cmdlet(VerbsData.Commit, "CmdletLogOnError")]
public class CommitCmdletLogOnError : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public string CmdletRequestId { get; set; }

    [Parameter(Mandatory = true)]
    public ErrorRecord ErrorRecord { get; set; }

    protected override void ProcessRecord()
    {
        DateTime endTime = DateTime.Now;

        LogColumn("GenericError", CmdletRequestId, ErrorRecord);
        LogColumn("EndTime", CmdletRequestId, endTime);

        CommitLog(CmdletRequestId);
    }
}

[Cmdlet(VerbsCommon.Execute, "CmdletEndBlock")]
public class ExecuteCmdletEndBlock : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public JObject BatchBodyObj { get; set; }

    [Parameter(Mandatory = true)]
    public string CmdletRequestId { get; set; }

    [Parameter(Mandatory = true)]
    public List<string> CmdletIDList { get; set; }

    protected override void ProcessRecord()
    {
        try
        {
            if (((JArray)BatchBodyObj["requests"]).Count > 0)
            {
                var batchResponse = ExecuteBatchRequest(CmdletRequestId);
                DeserializeBatchRequest(batchResponse, CmdletRequestId);
                
                // Resetting the batched requests as they are already handled
                BatchBodyObj["requests"] = new JArray();
                ResetBatchRequestParameters();
            }
        }
        catch (Exception ex)
        {
            CommitCmdletLogOnError(CmdletRequestId, new ErrorRecord(ex, "ExecuteCmdletEndBlockError", ErrorCategory.NotSpecified, this));
            throw;
        }

        DateTime endTime = DateTime.Now;
        LogColumn("EndTime", CmdletRequestId, endTime);

        foreach (var id in CmdletIDList)
        {
            CommitLog(id);
        }
    }
}

[Cmdlet(VerbsLifecycle.Execute, "CmdletBatchingBeginBody")]
public class ExecuteCmdletBatchingBeginBody : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public bool EnableBatchingStatus { get; set; }

    [Parameter(Mandatory = true)]
    public bool ExpectingInput { get; set; }

    protected override void ProcessRecord()
    {
        bool useBatching = false;

        // Use batching only if pipelined cmdlet is being executed
        if (EnableBatchingStatus && ExpectingInput)
        {
            useBatching = true;
        }

        var batchBodyObj = new JObject
        {
            ["requests"] = new JArray()
        };

        var batchRequestParameters = new List<object>();

        var result = new PSObject();
        result.Properties.Add(new PSNoteProperty("UseBatching", useBatching));
        result.Properties.Add(new PSNoteProperty("BatchBodyObj", batchBodyObj));
        result.Properties.Add(new PSNoteProperty("BatchRequestParameters", batchRequestParameters));

        WriteObject(result);
    }
}

[Cmdlet(VerbsCommunications.Write, "EndTimeInCmdletProcessBlock")]
public class LogEndTimeInCmdletProcessBlock : PSCmdlet
{
    [Parameter(Mandatory = true)]
    public bool UseBatching { get; set; }

    [Parameter(Mandatory = true)]
    public string CmdletRequestId { get; set; }

    protected override void ProcessRecord()
    {
        // For pipeline cases, EndProcessing is not called for each cmdlet in the pipeline, so we will log the end time here.
        // When batching is enabled, and the batch request is executed in this ProcessRecord, the log will be committed either inside Execute-Command or in the end block.
        if (!UseBatching)
        {
            DateTime endTime = DateTime.Now;
            LogColumn("EndTime", CmdletRequestId, endTime);
        }
    }
}
// endregion