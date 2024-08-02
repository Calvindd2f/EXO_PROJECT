// region Constants
//* ################################# Define all constants here ###################################
public const int DefaultHTTPTimeout = 960;              // 16 minutes, 1 minute more than server timeout so we don't timeout before the server.
public const int DefaultMaxRetryTimes = 5;              // In total we try 6 times. 1 original try, 5 retry attempts.
public const int DefaultRetryIntervalInMilliSeconds = 2000;
public const int DefaultRetryIntervalExponentBase = 2;
public const int DefaultPageSize = 1000;

public static readonly HttpStatusCode[] RetryableHTTPResponses = { HttpStatusCode.Unauthorized, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout };
$RetryableExceptions = @('DCOverloadedException', 'SuitabilityDirectoryException', 'ServerInMMException', 'ADServerSettingsChangedException', 'StorageTransientException')

public static readonly string[] RetryableExceptions = { "DCOverloadedException", "SuitabilityDirectoryException", "ServerInMMException", "ADServerSettingsChangedException", "StorageTransientException" };

public static readonly Dictionary<string, int> CmdletHttpTimeoutCollection = new Dictionary<string, int> {
    { "Get-DistributionGroupMember", 3600 },
    { "Import-TransportRuleCollection", 3600 },
    { "Set-OrganizationConfig", 1800 },
    { "Set-UnifiedGroup", 3600 },
    { "Update-DistributionGroupMember", 3600 },
    { "Update-RoleGroupMember", 1800 }
};

public static readonly string[] PowerShellParameters = { "Debug", "ErrorAction", "ErrorVariable", "OutBuffer", "OutVariable", "Verbose", "WarningAction", "WarningVariable", "InformationAction", "InformationVariable", "PipelineVariable" };

public static readonly string[] FilterParametersForBooleanParsing = { "Filter", "RecipientPreviewFilter", "RecipientFilter" };

public static readonly string[] StringsForBooleanParsing = { "$true", "($true)", "$true)", "$false", "($false)", "$false)" };

public static readonly string[] FilterWarningsInPagination = { "VGhlcmUgYXJlIG1vcmUgcmVzdWx0cyBhdmFpbGFibGUgdGhhbiBhcmUgY3VycmVudGx5IGRpc3BsYXllZC4gVG8gdmlldyB0aGVtLCBpbmNyZWFzZSB0aGUgdmFsdWUgZm9yIHRoZSBSZXN1bHRTaXplIHBhcmFtZXRlci4=" };

public const string FilterWarningsInPaginationDecoded = "";

public static bool EXO_LastExecutionStatus = true;

// endregion
//* ################################################################################################
