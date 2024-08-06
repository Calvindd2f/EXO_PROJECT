using System;
using Exchange.ConstantFunctions;
using Exchange.GlobalVariables;
using Exchange.Constants;
using System.Collections.Generic;
using System.Management.Automation;

namespace Exchange.Functions.CalendarsAndGroups
{
    [Cmdlet(VerbsCommon.Get, "AvailabilityAddressSpace")]
    public class AddAvailabilityAddressSpace : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "DistributionGroupMember")]
    public class AddDistributionGroupMember : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "MailboxFolderPermission")]
    public class AddMailboxFolderPermission : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "MailboxDiagnosticLogs")]
    public class ExportMailboxDiagnosticLogs : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "AvailabilityAddressSpace")]
    public class GetAvailabilityAddressSpace : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "AvailabilityConfig")]
    public class GetAvailabilityConfig : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "CalendarDiagnosticAnalysis")]
    public class GetCalendarDiagnosticAnalysis : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "CalendarDiagnosticLog")]
    public class GetCalendarDiagnosticLog : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "CalendarDiagnosticObjects")]
    public class GetCalendarDiagnosticObjects : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "CalendarProcessing")]
    public class GetCalendarProcessing : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "DistributionGroup")]
    public class GetDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "DistributionGroupMember")]
    public class GetDistributionGroupMember : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "DynamicDistributionGroup")]
    public class GetDynamicDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "EligibleDistributionGroupForMigration")]
    public class GetEligibleDistributionGroupForMigration : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "FederatedOrganizationIdentifier")]
    public class GetFederatedOrganizationIdentifier : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "FederationInformation")]
    public class GetFederationInformation : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "FederationTrust")]
    public class GetFederationTrust : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "Group")]
    public class GetGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "IntraOrganizationConfiguration")]
    public class GetIntraOrganizationConfiguration : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "IntraOrganizationConnector")]
    public class GetIntraOrganizationConnector : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "MailboxCalendarFolder")]
    public class GetMailboxCalendarFolder : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "MailboxFolderPermission")]
    public class GetMailboxFolderPermission : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "OnlineMeetingConfiguration")]
    public class GetOnlineMeetingConfiguration : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "OrganizationRelationship")]
    public class GetOrganizationRelationship : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "Place")]
    public class GetPlace : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "PlacesSetting")]
    public class GetPlacesSettings : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "PlacesSettings")]
    public class SetPlacesSettings : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "ResourceConfig")]
    public class GetResourceConfig : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Get, "SharingPolicy")]
    public class GetSharingPolicy : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.New, "AvailabilityConfig")]
    public class NewAvailabilityConfig : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.New, "DistributionGroup")]
    public class NewDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.New, "DynamicDistributionGroup")]
    public class NewDynamicDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.New, "IntraOrganizationConnector")]
    public class NewIntraOrganizationConnector : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.New, "OrganizationRelationship")]
    public class NewOrganizationRelationship : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.New, "SharingPolicy")]
    public class NewSharingPolicy : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "AvailabilityAddressSpace")]
    public class RemoveAvailabilityAddressSpace : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "AvailabilityConfig")]
    public class RemoveAvailabilityConfig : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "CalendarEvents")]
    public class RemoveCalendarEvents : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "DistributionGroup")]
    public class RemoveDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "DistributionGroupMember")]
    public class RemoveDistributionGroupMember : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "DynamicDistributionGrou")]
    public class RemoveDynamicDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "IntraOrganizationConnector")]
    public class RemoveIntraOrganizationConnector : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "MailboxFolderPermission")]
    public class RemoveMailboxFolderPermission : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "OrganizationRelationship")]
    public class RemoveOrganizationRelationship : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Remove, "SharingPolicy")]
    public class RemoveSharingPolicy : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "AvailabilityConfig")]
    public class SetAvailabilityConfig : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "CalendarProcessing")]
    public class SetCalendarProcessing : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "DistributionGroup")]
    public class SetDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "DynamicDistributionGroup")]
    public class SetDynamicDistributionGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "FederatedOrganizationIdentifier")]
    public class SetFederatedOrganizationIdentifier : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "Group")]
    public class SetGroup : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "IntraOrganizationConnector")]
    public class SetIntraOrganizationConnector : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "MailboxCalendarFolder")]
    public class SetMailboxCalendarFolder : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "MailboxFolderPermission")]
    public class SetMailboxFolderPermission : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "OrganizationRelationship")]
    public class SetOrganizationRelationship : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "Place")]
    public class SetPlace : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "ResourceConfig")]
    public class SetResourceConfig : PSCmdlet
	{ }
    
    [Cmdlet(VerbsCommon.Set, "SharingPolicy")]
    public class SetSharingPolicy : PSCmdlet
	{ }
    
    public class TestOrganizationRelationship : PSCmdlet
	{ }
    
    public class UpdateDistributionGroupMember : PSCmdlet
	{ }
    
    public class UpgradeDistributionGroup : PSCmdlet
	{ }
}
