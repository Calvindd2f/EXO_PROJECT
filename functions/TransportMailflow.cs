using System;
using Exchange.ConstantFunctions;
using Exchange.GlobalVariables;
using Exchange.Constants;
using System.Collections.Generic;
using System.Management.Automation;

namespace Exchange.functions.TransportMailflow
{
    public class DisableAntiPhishRule : PSCmdlet
    { }
    
    public class DisableHostedContentFilterRule : PSCmdlet
    { }
    
    public class DisableHostedOutboundSpamFilterRule : PSCmdlet
    { }
    
    public class DisableInboxRule : PSCmdlet
    { }
    
    public class DisableMalwareFilterRule : PSCmdlet
    { }
    
    public class DisableSafeAttachmentRule : PSCmdlet
    { }
    
    public class DisableSafeLinksRule : PSCmdlet
    { }
    
    public class DisableSweepRule : PSCmdlet
    { }
    
    public class DisableTransportRule : PSCmdlet
    { }
    
    public class EnableAntiPhishRule : PSCmdlet
    { }
    
    public class EnableComplianceTagStorage : PSCmdlet
    { }
    
    public class EnableHostedContentFilterRule : PSCmdlet
    { }
    
    public class EnableHostedOutboundSpamFilterRule : PSCmdlet
    { }
    
    public class EnableInboxRule : PSCmdlet
    { }
    
    public class EnableMalwareFilterRule : PSCmdlet
    { }
    
    public class EnableSafeAttachmentRule : PSCmdlet
    { }
    
    public class EnableSafeLinksRule : PSCmdlet
    { }
    
    public class EnableSweepRule : PSCmdlet
    { }
    
    public class EnableTransportRule : PSCmdlet
    { }
    
    public class ExecuteAzureADLabelSync : PSCmdlet
    { }
    
    public class ExportDlpPolicyCollection : PSCmdlet
    { }
    
    public class ExportTransportRuleCollection : PSCmdlet
    { }
    
    public class GetActivityAlert : PSCmdlet
    { }
    
    public class GetAntiPhishPolicy : PSCmdlet
    { }
    
    public class GetAntiPhishRule : PSCmdlet
    { }
    
    public class GetAtpPolicyForO365 : PSCmdlet
    { }
    
    public class GetAuditConfig : PSCmdlet
    { }
    
    public class GetAuditConfigurationPolicy : PSCmdlet
    { }
    
    public class GetAuditConfigurationRule : PSCmdlet
    { }
    
    public class GetAutoSensitivityLabelPolicy : PSCmdlet
    { }
    
    public class GetAutoSensitivityLabelRule : PSCmdlet
    { }
    
    public class GetBlockedSenderAddress : PSCmdlet
    { }
    
    public class GetCaseHoldPolicy : PSCmdlet
    { }
    
    public class GetCaseHoldRule : PSCmdlet
    { }
    
    public class GetComplianceRetentionEvent : PSCmdlet
    { }
    
    public class GetComplianceRetentionEventType : PSCmdlet
    { }
    
    public class GetComplianceTag : PSCmdlet
    { }
    
    public class GetComplianceTagStorage : PSCmdlet
    { }
    
    public class GetDeviceConditionalAccessPolicy : PSCmdlet
    { }
    
    public class GetDeviceConditionalAccessRule : PSCmdlet
    { }
    
    public class GetDeviceConfigurationPolicy : PSCmdlet
    { }
    
    public class GetDeviceConfigurationRule : PSCmdlet
    { }
    
    public class GetDevicePolicy : PSCmdlet
    { }
    
    public class GetDeviceTenantPolicy : PSCmdlet
    { }
    
    public class GetDeviceTenantRule : PSCmdlet
    { }
    
    public class GetDkimSigningConfig : PSCmdlet
    { }
    
    public class GetDlpCompliancePolicy : PSCmdlet
    { }
    
    public class GetDlpComplianceRule : PSCmdlet
    { }
    
    public class GetDlpEdmSchema : PSCmdlet
    { }
    
    public class GetDlpKeywordDictionary : PSCmdlet
    { }
    
    public class GetDlpPolicy : PSCmdlet
    { }
    
    public class GetDlpPolicyTemplate : PSCmdlet
    { }
    
    public class GetDlpSensitiveInformationType : PSCmdlet
    { }
    
    public class GetDlpSensitiveInformationTypeRulePackage : PSCmdlet
    { }
    
    public class GetHoldCompliancePolicy : PSCmdlet
    { }
    
    public class GetHoldComplianceRule : PSCmdlet
    { }
    
    public class GetHostedConnectionFilterPolicy : PSCmdlet
    { }
    
    public class GetHostedContentFilterPolicy : PSCmdlet
    { }
    
    public class GetHostedContentFilterRule : PSCmdlet
    { }
    
    public class GetHostedOutboundSpamFilterPolicy : PSCmdlet
    { }
    
    public class GetHostedOutboundSpamFilterRule : PSCmdlet
    { }
    
    public class GetInboundConnector : PSCmdlet
    { }
    
    public class GetInboxRule : PSCmdlet
    { }
    
    public class GetInformationBarrierPoliciesApplicationStatus : PSCmdlet
    { }
    
    public class GetInformationBarrierPolicy : PSCmdlet
    { }
    
    public class GetInformationBarrierRecipientStatus : PSCmdlet
    { }
    
    public class GetLabel : PSCmdlet
    { }
    
    public class GetLabelPolicy : PSCmdlet
    { }
    
    public class GetMalwareFilterPolicy : PSCmdlet
    { }
    
    public class GetMalwareFilterRule : PSCmdlet
    { }
    
    public class GetMessageCategory : PSCmdlet
    { }
    
    public class GetOutboundConnector : PSCmdlet
    { }
    
    public class GetPerimeterConfig : PSCmdlet
    { }
    
    public class GetPolicyTipConfig : PSCmdlet
    { }
    
    public class GetProtectionAlert : PSCmdlet
    { }
    
    public class GetRegulatoryComplianceUI : PSCmdlet
    { }
    
    public class GetRetentionCompliancePolicy : PSCmdlet
    { }
    
    public class GetRetentionComplianceRule : PSCmdlet
    { }
    
    public class GetRetentionEvent : PSCmdlet
    { }
    
    public class GetSafeAttachmentPolicy : PSCmdlet
    { }
    
    public class GetSafeAttachmentRule : PSCmdlet
    { }
    
    public class GetSafeLinksPolicy : PSCmdlet
    { }
    
    public class GetSafeLinksRule : PSCmdlet
    { }
    
    public class GetSupervisoryReviewActivity : PSCmdlet
    { }
    
    public class GetSupervisoryReviewPolicyV2 : PSCmdlet
    { }
    
    public class GetSupervisoryReviewRule : PSCmdlet
    { }
    
    public class GetSweepRule : PSCmdlet
    { }
    
    public class GetTenantAllowBlockListItems : PSCmdlet
    { }
    
    public class GetTransportRule : PSCmdlet
    { }
    
    public class GetTransportRuleAction : PSCmdlet
    { }
    
    public class GetTransportRulePredicate : PSCmdlet
    { }
    
    public class GetUnifiedAuditLogRetentionPolicy : PSCmdlet
    { }
    
    public class ImportDlpPolicyCollection : PSCmdlet
    { }
    
    public class ImportTransportRuleCollection : PSCmdlet
    { }
    
    public class InstallUnifiedCompliancePrerequisite : PSCmdlet
    { }
    
    public class InvokeHoldRemovalAction : PSCmdlet
    { }
    
    public class NewActivityAlert : PSCmdlet
    { }
    
    public class NewAntiPhishPolicy : PSCmdlet
    { }
    
    public class NewAntiPhishRule : PSCmdlet
    { }
    
    public class NewAuditConfigurationPolicy : PSCmdlet
    { }
    
    public class NewAuditConfigurationRule : PSCmdlet
    { }
    
    public class NewAutoSensitivityLabelPolicy : PSCmdlet
    { }
    
    public class NewAutoSensitivityLabelRule : PSCmdlet
    { }
    
    public class NewCaseHoldPolicy : PSCmdlet
    { }
    
    public class NewCaseHoldRule : PSCmdlet
    { }
    
    public class NewComplianceRetentionEvent : PSCmdlet
    { }
    
    public class NewComplianceRetentionEventType : PSCmdlet
    { }
    
    public class NewComplianceTag : PSCmdlet
    { }
    
    public class NewDeviceConditionalAccessPolicy : PSCmdlet
    { }
    
    public class NewDeviceConditionalAccessRule : PSCmdlet
    { }
    
    public class NewDeviceConfigurationPolicy : PSCmdlet
    { }
    
    public class NewDeviceConfigurationRule : PSCmdlet
    { }
    
    public class NewDeviceTenantPolicy : PSCmdlet
    { }
    
    public class NewDeviceTenantRule : PSCmdlet
    { }
    
    public class NewDkimSigningConfig : PSCmdlet
    { }
    
    public class NewDlpCompliancePolicy : PSCmdlet
    { }
    
    public class NewDlpComplianceRule : PSCmdlet
    { }
    
    public class NewDlpEdmSchema : PSCmdlet
    { }
    
    public class NewDlpFingerprint : PSCmdlet
    { }
    
    public class NewDlpKeywordDictionary : PSCmdlet
    { }
    
    public class NewDlpPolicy : PSCmdlet
    { }
    
    public class NewDlpSensitiveInformationType : PSCmdlet
    { }
    
    public class NewDlpSensitiveInformationTypeRulePackage : PSCmdlet
    { }
    
    public class NewHoldCompliancePolicy : PSCmdlet
    { }
    
    public class NewHoldComplianceRule : PSCmdlet
    { }
    
    public class NewHostedContentFilterPolicy : PSCmdlet
    { }
    
    public class NewHostedContentFilterRule : PSCmdlet
    { }
    
    public class NewHostedOutboundSpamFilterPolicy : PSCmdlet
    { }
    
    public class NewHostedOutboundSpamFilterRule : PSCmdlet
    { }
    
    public class NewInboundConnector : PSCmdlet
    { }
    
    public class NewInboxRule : PSCmdlet
    { }
    
    public class NewInformationBarrierPolicy : PSCmdlet
    { }
    
    public class NewLabel : PSCmdlet
    { }
    
    public class NewLabelPolicy : PSCmdlet
    { }
    
    public class NewMalwareFilterPolicy : PSCmdlet
    { }
    
    public class NewMalwareFilterRule : PSCmdlet
    { }
    
    public class NewOutboundConnector : PSCmdlet
    { }
    
    public class NewPolicyTipConfig : PSCmdlet
    { }
    
    public class NewProtectionAlert : PSCmdlet
    { }
    
    public class NewRetentionCompliancePolicy : PSCmdlet
    { }
    
    public class NewRetentionComplianceRule : PSCmdlet
    { }
    
    public class NewSafeAttachmentPolicy : PSCmdlet
    { }
    
    public class NewSafeAttachmentRule : PSCmdlet
    { }
    
    public class NewSafeLinksPolicy : PSCmdlet
    { }
    
    public class NewSafeLinksRule : PSCmdlet
    { }
    
    public class NewSupervisoryReviewPolicyV2 : PSCmdlet
    { }
    
    public class NewSupervisoryReviewRule : PSCmdlet
    { }
    
    public class NewSweepRule : PSCmdlet
    { }
    
    public class NewTenantAllowBlockListItems : PSCmdlet
    { }
    
    public class NewTransportRule : PSCmdlet
    { }
    
    public class NewUnifiedAuditLogRetentionPolicy : PSCmdlet
    { }
    
    public class RemoveActivityAlert : PSCmdlet
    { }
    
    public class RemoveAntiPhishPolicy : PSCmdlet
    { }
    
    public class RemoveAntiPhishRule : PSCmdlet
    { }
    
    public class RemoveAuditConfigurationPolicy : PSCmdlet
    { }
    
    public class RemoveAuditConfigurationRule : PSCmdlet
    { }
    
    public class RemoveAutoSensitivityLabelPolicy : PSCmdlet
    { }
    
    public class RemoveAutoSensitivityLabelRule : PSCmdlet
    { }
    
    public class RemoveBlockedSenderAddress : PSCmdlet
    { }
    
    public class RemoveCaseHoldPolicy : PSCmdlet
    { }
    
    public class RemoveCaseHoldRule : PSCmdlet
    { }
    
    public class RemoveComplianceRetentionEventType : PSCmdlet
    { }
    
    public class RemoveComplianceTag : PSCmdlet
    { }
    
    public class RemoveDeviceConditionalAccessPolicy : PSCmdlet
    { }
    
    public class RemoveDeviceConditionalAccessRule : PSCmdlet
    { }
    
    public class RemoveDeviceConfigurationPolicy : PSCmdlet
    { }
    
    public class RemoveDeviceConfigurationRule : PSCmdlet
    { }
    
    public class RemoveDeviceTenantPolicy : PSCmdlet
    { }
    
    public class RemoveDeviceTenantRule : PSCmdlet
    { }
    
    public class RemoveDlpCompliancePolicy : PSCmdlet
    { }
    
    public class RemoveDlpComplianceRule : PSCmdlet
    { }
    
    public class RemoveDlpEdmSchema : PSCmdlet
    { }
    
    public class RemoveDlpKeywordDictionary : PSCmdlet
    { }
    
    public class RemoveDlpPolicy : PSCmdlet
    { }
    
    public class RemoveDlpSensitiveInformationType : PSCmdlet
    { }
    
    public class RemoveDlpSensitiveInformationTypeRulePackage : PSCmdlet
    { }
    
    public class RemoveHoldCompliancePolicy : PSCmdlet
    { }
    
    public class RemoveHoldComplianceRule : PSCmdlet
    { }
    
    public class RemoveHostedContentFilterPolicy : PSCmdlet
    { }
    
    public class RemoveHostedContentFilterRule : PSCmdlet
    { }
    
    public class RemoveHostedOutboundSpamFilterPolicy : PSCmdlet
    { }
    
    public class RemoveHostedOutboundSpamFilterRule : PSCmdlet
    { }
    
    public class RemoveInboundConnector : PSCmdlet
    { }
    
    public class RemoveInboxRule : PSCmdlet
    { }
    
    public class RemoveInformationBarrierPolicy : PSCmdlet
    { }
    
    public class RemoveLabel : PSCmdlet
    { }
    
    public class RemoveLabelPolicy : PSCmdlet
    { }
    
    public class RemoveMalwareFilterPolicy : PSCmdlet
    { }
    
    public class RemoveMalwareFilterRule : PSCmdlet
    { }
    
    public class RemoveOutboundConnector : PSCmdlet
    { }
    
    public class RemovePolicyTipConfig : PSCmdlet
    { }
    
    public class RemoveProtectionAlert : PSCmdlet
    { }
    
    public class RemoveRecordLabel : PSCmdlet
    { }
    
    public class RemoveRetentionCompliancePolicy : PSCmdlet
    { }
    
    public class RemoveRetentionComplianceRule : PSCmdlet
    { }
    
    public class RemoveSafeAttachmentPolicy : PSCmdlet
    { }
    
    public class RemoveSafeAttachmentRule : PSCmdlet
    { }
    
    public class RemoveSafeLinksPolicy : PSCmdlet
    { }
    
    public class RemoveSafeLinksRule : PSCmdlet
    { }
    
    public class RemoveSupervisoryReviewPolicyV2 : PSCmdlet
    { }
    
    public class RemoveSweepRule : PSCmdlet
    { }
    
    public class RemoveTenantAllowBlockListItems : PSCmdlet
    { }
    
    public class RemoveTransportRule : PSCmdlet
    { }
    
    public class RemoveUnifiedAuditLogRetentionPolicy : PSCmdlet
    { }
    
    public class RotateDkimSigningConfig : PSCmdlet
    { }
    
    public class SearchMessageTrackingReport : PSCmdlet
    { }
    
    public class SetActivityAlert : PSCmdlet
    { }
    
    public class SetAntiPhishPolicy : PSCmdlet
    { }
    
    public class SetAntiPhishRule : PSCmdlet
    { }
    
    public class SetAtpPolicyForO365 : PSCmdlet
    { }
    
    public class SetAuditConfig : PSCmdlet
    { }
    
    public class SetAuditConfigurationRule : PSCmdlet
    { }
    
    public class SetAutoSensitivityLabelPolicy : PSCmdlet
    { }
    
    public class SetAutoSensitivityLabelRule : PSCmdlet
    { }
    
    public class SetCaseHoldPolicy : PSCmdlet
    { }
    
    public class SetCaseHoldRule : PSCmdlet
    { }
    
    public class SetComplianceRetentionEventType : PSCmdlet
    { }
    
    public class SetComplianceTag : PSCmdlet
    { }
    
    public class SetDeviceConditionalAccessPolicy : PSCmdlet
    { }
    
    public class SetDeviceConditionalAccessRule : PSCmdlet
    { }
    
    public class SetDeviceConfigurationPolicy : PSCmdlet
    { }
    
    public class SetDeviceConfigurationRule : PSCmdlet
    { }
    
    public class SetDeviceTenantPolicy : PSCmdlet
    { }
    
    public class SetDeviceTenantRule : PSCmdlet
    { }
    
    public class SetDkimSigningConfig : PSCmdlet
    { }
    
    public class SetDlpCompliancePolicy : PSCmdlet
    { }
    
    public class SetDlpComplianceRule : PSCmdlet
    { }
    
    public class SetDlpEdmSchema : PSCmdlet
    { }
    
    public class SetDlpKeywordDictionary : PSCmdlet
    { }
    
    public class SetDlpPolicy : PSCmdlet
    { }
    
    public class SetDlpSensitiveInformationType : PSCmdlet
    { }
    
    public class SetDlpSensitiveInformationTypeRulePackage : PSCmdlet
    { }
    
    public class SetHoldCompliancePolicy : PSCmdlet
    { }
    
    public class SetHoldComplianceRule : PSCmdlet
    { }
    
    public class SetHostedConnectionFilterPolicy : PSCmdlet
    { }
    
    public class SetHostedContentFilterPolicy : PSCmdlet
    { }
    
    public class SetHostedContentFilterRule : PSCmdlet
    { }
    
    public class SetHostedOutboundSpamFilterPolicy : PSCmdlet
    { }
    
    public class SetHostedOutboundSpamFilterRule : PSCmdlet
    { }
    
    public class SetInboundConnector : PSCmdlet
    { }
    
    public class SetInboxRule : PSCmdlet
    { }
    
    public class SetInformationBarrierPolicy : PSCmdlet
    { }
    
    public class SetLabel : PSCmdlet
    { }
    
    public class SetLabelPolicy : PSCmdlet
    { }
    
    public class SetMalwareFilterPolicy : PSCmdlet
    { }
    
    public class SetMalwareFilterRule : PSCmdlet
    { }
    
    public class SetOutboundConnector : PSCmdlet
    { }
    
    public class SetPerimeterConfig : PSCmdlet
    { }
    
    public class SetPolicyTipConfig : PSCmdlet
    { }
    
    public class SetProtectionAlert : PSCmdlet
    { }
    
    public class SetRegulatoryComplianceUI : PSCmdlet
    { }
    
    public class SetRetentionCompliancePolicy : PSCmdlet
    { }
    
    public class SetRetentionComplianceRule : PSCmdlet
    { }
    
    public class SetSafeAttachmentPolicy : PSCmdlet
    { }
    
    public class SetSafeAttachmentRule : PSCmdlet
    { }
    
    public class SetSafeLinksPolicy : PSCmdlet
    { }
    
    public class SetSafeLinksRule : PSCmdlet
    { }
    
    public class SetSupervisoryReviewPolicyV2 : PSCmdlet
    { }
    
    public class SetSupervisoryReviewRule : PSCmdlet
    { }
    
    public class SetSweepRule : PSCmdlet
    { }
    
    public class SetTenantAllowBlockListItems : PSCmdlet
    { }
    
    public class SetTransportRule : PSCmdlet
    { }
    
    public class SetUnifiedAuditLogRetentionPolicy : PSCmdlet
    { }
    
    public class StartInformationBarrierPoliciesApplication : PSCmdlet
    { }
    
    public class StopInformationBarrierPoliciesApplication : PSCmdlet
    { }
    
    public class ValidateOutboundConnector : PSCmdlet
    { }
    
    public class ValidateRetentionRuleQuery : PSCmdlet
    { }
    
}