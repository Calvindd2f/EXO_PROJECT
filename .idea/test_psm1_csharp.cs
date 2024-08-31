using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.IO.Pipes;
using System.IO.Enumeration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Collections;



#region Constants
/*################################# Define all constants here ###################################*/
public const int DefaultTimeout = 960; // 16 minutes, 1 minute more than server timeout
public const int DefaultMaxRetryTimes = 5; // In total we try 6 times: 1 original + 5 retries
public const int DefaultRetryIntervalInMilliseconds = 2000;
public const int DefaultRetryIntervalExponentBase = 2;
public const int DefaultPageSize = 1000;
public const int DefaultRetryLimitForSeverSideException = 2;
public static readonly HttpStatusCode[] RetryableResponses = new[]
{
        HttpStatusCode.Unauthorized,
        HttpStatusCode.ServiceUnavailable,
        HttpStatusCode.GatewayTimeout
    };
public static readonly string[] RetryableExceptions = new[]
{
        "DCOverloadedException",
        "SuitabilityDirectoryException",
        "ServerInMMException",
        "ADServerSettingsChangedException",
        "StorageTransientException"
    };
public static readonly Dictionary<string, int> CmdletHttpTimeoutCollection = new Dictionary<string, int>
    {
        { "Get-DistributionGroupMember", 3600 },
        { "Import-TransportRuleCollection", 3600 },
        { "Set-OrganizationConfig", 1800 },
        { "Set-UnifiedGroup", 3600 },
        { "Update-DistributionGroupMember", 3600 },
        { "Update-RoleGroupMember", 1800 }
    };
public static readonly string[] CommonParameters =
{
        "Debug", "ErrorAction", "ErrorVariable", "OutBuffer", "OutVariable",
        "Verbose", "WarningAction", "WarningVariable", "InformationAction",
        "InformationVariable", "PipelineVariable"
    };
public static readonly string[] FilterParametersForBooleanParsing =
{
        "Filter", "RecipientPreviewFilter", "RecipientFilter"
    };
public static readonly string[] StringsForBooleanParsing =
{
        "$true", "($true)", "$true)", "$false", "($false)", "$false)"
    };
public static readonly string[] FilterWarningsEncoded =
{
           "VGhlcmUgYXJlIG1vcmUgcmVzdWx0cyBhdmFpbGFibGUgdGhhbiBhcmUgY3VycmVudGx5IGRpc3BsYXllZC4gVG8gdmlldyB0aGVtLCBpbmNyZWFzZSB0aGUgdmFsdWUgZm9yIHRoZSBSZXN1bHRTaXplIHBhcmFtZXRlci4="
    };
public static readonly string FilterWarningsInPaginationDecoded = string.Empty;
public static bool EXO_LastExecutionStatus = true;
/*################################################################################################*/
#endregion

#region Global Variables
[Cmdlet(VerbsLifecycle.Should, "UseCustomRoutingByDefault")]
[OutputType(typeof(bool))]
public class ShouldUseCustomRoutingByDefaultCommand : Cmdlet
{
    protected override void ProcessRecord()
    {
        WriteObject(false);
    }
}
#endregion

//#region Constant Functions

[Cmdlet(VerbsCommon.Get, "CurrentConnectionContext")]
public class GetCurrentConnectionContextCommand : PSCmdlet
{
    [Parameter]
    public string ConnectionContextObjectId { get; set; }

    protected override void ProcessRecord()
    {
        if (!string.IsNullOrEmpty(ConnectionContextObjectId))
        {
            WriteObject(Microsoft.Exchange.Management.ExoPowershellSnapin.ConnectionContextFactory.GetCurrentConnectionContext(ConnectionContextObjectId));
        }
        else
        {
            WriteObject(null);
        }
    }
}

/// This function tries to get the SortableDateTimePattern from the Get-Culture command. 
/// If Get-Culture returns null, static value of SortableDateTimePattern is used. 
[Cmdlet(VerbsCommon.Get, "SortableDateTimePattern")]
public class GetSortableDateTimePatternCommand : PSCmdlet
{
    protected override void ProcessRecord()
    {
        var cultureInfo = System.Globalization.CultureInfo.CurrentCulture;
        if (cultureInfo != null)
        {
            var cultureDateTimeFormat = cultureInfo.DateTimeFormat;
            if (cultureDateTimeFormat != null)
            {
                var sortableCultureDateTimeFormat = cultureDateTimeFormat.SortableDateTimePattern;
                if (!string.IsNullOrEmpty(sortableCultureDateTimeFormat))
                {
                    WriteObject(sortableCultureDateTimeFormat);
                    return;
                }
            }
        }
        WriteObject("yyyy'-'MM'-'dd'T'HH':'mm':'ss");
    }
}

public enum Column
{
    CmdletName,
    CmdletParameters,
    StartTime,
    EndTime,
    GenericInfo,
    GenericError
}

[Cmdlet(VerbsOther.Log, "Column")]
public class LogColumnCommand : PSCmdlet
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
                if (!(Value is string) && !(Value is Dictionary<string, object>))
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
                if (!(Value is string) && !(Value is ErrorRecord))
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

}

[Cmdlet(VerbsData.Initialize, "Log")]
public class InitLogCommand : PSCmdlet
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

        dynamic logger = connectionContext.Logger;
        if (logger != null)
        {
            logger.InitLog(CmdletId);
        }
    }
}

[Cmdlet(VerbsData.Commit, "Log")]
public class InitLogCommand : PSCmdlet
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

        dynamic logger = connectionContext.Logger;
        if (logger != null)
        {
            logger.CommitLog(CmdletId);
        }
    }
}

// Same function is present in the ExchangeOnlineManagement.psm1 module. Both the codes should be kept in sync.
[Cmdlet(VerbsCommon.Get, "ModuleVersion")]
public class GetModuleVersion : Cmdlet
{
    protected override void BeginProcessing()
    {
        // Return the already computed version info if available.
        
        if ((ModuleVersion != $null) && (ModuleVersion != ''))
        {
            WriteVerbose("Returning precomputed version info: $script:ModuleVersion");
            return ModuleVersion;
        }

        var exoModule = //TODO(@Calvindd2f): To be continued.


    }


}

























#endregion

// Classes and PSCmdlets from the functions namespace

/******************************************************************************/
private string TokenProviderObjectId = '6135f2a0-43b9-440a-8a49-b54ee0060483';
private string ConnectionContextObjectId = '28be6dc3-1644-421e-8af3-3a5cd946418e';