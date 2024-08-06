using System;
using Exchange.ConstantFunctions;
using Exchange.GlobalVariables;
using Exchange.Constants;
using System.Collections.Generic;
using System.Management.Automation;

namespace Exchange.Functions.RolesAndAccess
{
    public class AddMailboxPermission : PSCmdlet
    {}
    
    public class AddManagementRoleEntry : PSCmdlet
    {}
    
    public class AddRoleGroupMember : PSCmdlet
    {}
    
    public class AddUnifiedGroupLinks : PSCmdlet
    {}
    
    public class DisableJournalArchiving : PSCmdlet
    {}
    
    public class DisableMailbox : PSCmdlet
    {}
    
    public class EnableMailbox : PSCmdlet
    {}
    
    public class GetAccessToCustomerDataRequest : PSCmdlet
    {}
    
    public class GetAddressBookPolicy : PSCmdlet
    {}
    
    public class GetAddressList : PSCmdlet
    {}
    
    public class GetAdministrativeUnit : PSCmdlet
    {}
    
    public class GetAuthenticationPolicy : PSCmdlet
    {}
    
    public class GetAuthServer : PSCmdlet
    {}
    
    public class GetContact : PSCmdlet
    {}
    
    public class GetEmailAddressPolicy : PSCmdlet
    {}
    
    public class GetEventsFromEmailConfiguration : PSCmdlet
    {}
    
    public class GetGlobalAddressList : PSCmdlet
    {}
    
    public class GetLinkedUser : PSCmdlet
    {}
    
    public class GetMailbox : PSCmdlet
    {}
    
    public class GetMailboxAutoReplyConfiguration : PSCmdlet
    {}
    
    public class GetMailboxCalendarConfiguration : PSCmdlet
    {}
    
    public class GetMailboxFolder : PSCmdlet
    {}
    
    public class GetMailboxJunkEmailConfiguration : PSCmdlet
    {}
    
    public class GetMailboxLocation : PSCmdlet
    {}
    
    public class GetMailboxMessageConfiguration : PSCmdlet
    {}
    
    public class GetMailboxPermission : PSCmdlet
    {}
    
    public class GetMailboxRegionalConfiguration : PSCmdlet
    {}
    
    public class GetMailboxSpellingConfiguration : PSCmdlet
    {}
    
    public class GetMailboxUserConfiguration : PSCmdlet
    {}
    
    public class GetMailContact : PSCmdlet
    {}
    
    public class GetMailUser : PSCmdlet
    {}
    
    public class GetManagementRole : PSCmdlet
    {}
    
    public class GetManagementRoleAssignment : PSCmdlet
    {}
    
    public class GetManagementRoleEntry : PSCmdlet
    {}
    
    public class GetManagementScope : PSCmdlet
    {}
    
    public class GetOfflineAddressBook : PSCmdlet
    {}
    
    public class GetOnPremisesOrganization : PSCmdlet
    {}
    
    public class GetPartnerApplication : PSCmdlet
    {}
    
    public class GetRecipient : PSCmdlet
    {}
    
    public class GetRoleAssignmentPolicy : PSCmdlet
    {}
    
    public class GetRoleGroup : PSCmdlet
    {}
    
    public class GetRoleGroupMember : PSCmdlet
    {}
    
    public class GetSiteMailbox : PSCmdlet
    {}
    
    public class GetSiteMailboxDiagnostics : PSCmdlet
    {}
    
    public class GetTenantAnalyticsConfig : PSCmdlet
    {}
    
    public class GetUnifiedGroup : PSCmdlet
    {}
    
    public class GetUnifiedGroupLinks : PSCmdlet
    {}
    
    public class GetUser : PSCmdlet
    {}
    
    public class GetUserAnalyticsConfig : PSCmdlet
    {}
    
    public class GetUserPhoto : PSCmdlet
    {}
    
    public class ImportRecipientDataProperty : PSCmdlet
    {}
    
    public class NewAddressBookPolicy : PSCmdlet
    {}
    
    public class NewAddressList : PSCmdlet
    {}
    
    public class NewAuthenticationPolicy : PSCmdlet
    {}
    
    public class NewEmailAddressPolicy : PSCmdlet
    {}
    
    public class NewEOPDistributionGroup : PSCmdlet
    {}
    
    public class NewEOPMailUser : PSCmdlet
    {}
    
    public class NewGlobalAddressList : PSCmdlet
    {}
    
    public class NewMailbox : PSCmdlet
    {}
    
    public class NewMailboxFolder : PSCmdlet
    {}
    
    public class NewMailContact : PSCmdlet
    {}
    
    public class NewMailUser : PSCmdlet
    {}
    
    public class NewManagementRole : PSCmdlet
    {}
    
    public class NewManagementRoleAssignment : PSCmdlet
    {}
    
    public class NewManagementScope : PSCmdlet
    {}
    
    public class NewOfflineAddressBook : PSCmdlet
    {}
    
    public class NewOnPremisesOrganization : PSCmdlet
    {}
    
    public class NewPartnerApplication : PSCmdlet
    {}
    
    public class NewRoleAssignmentPolicy : PSCmdlet
    {}
    
    public class NewRoleGroup : PSCmdlet
    {}
    
    public class NewSiteMailbox : PSCmdlet
    {}
    
    public class NewUnifiedGroup : PSCmdlet
    {}
    
    public class RemoveAddressBookPolicy : PSCmdlet
    {}
    
    public class RemoveAddressList : PSCmdlet
    {}
    
    public class RemoveAuthenticationPolicy : PSCmdlet
    {}
    
    public class RemoveEmailAddressPolicy : PSCmdlet
    {}
    
    public class RemoveEOPDistributionGroup : PSCmdlet
    {}
    
    public class RemoveEOPMailUser : PSCmdlet
    {}
    
    public class RemoveGlobalAddressList : PSCmdlet
    {}
    
    public class RemoveHybridConfiguration : PSCmdlet
    {}
    
    public class RemoveMailbox : PSCmdlet
    {}
    
    public class RemoveMailboxPermission : PSCmdlet
    {}
    
    public class RemoveMailboxUserConfiguration : PSCmdlet
    {}
    
    public class RemoveMailContact : PSCmdlet
    {}
    
    public class RemoveMailUser : PSCmdlet
    {}
    
    public class RemoveManagementRole : PSCmdlet
    {}
    
    public class RemoveManagementRoleAssignment : PSCmdlet
    {}
    
    public class RemoveManagementRoleEntry : PSCmdlet
    {}
    
    public class RemoveManagementScope : PSCmdlet
    {}
    
    public class RemoveOfflineAddressBook : PSCmdlet
    {}
    
    public class RemoveOnPremisesOrganization : PSCmdlet
    {}
    
    public class RemovePartnerApplication : PSCmdlet
    {}
    
    public class RemoveRoleAssignmentPolicy : PSCmdlet
    {}
    
    public class RemoveRoleGroup : PSCmdlet
    {}
    
    public class RemoveRoleGroupMember : PSCmdlet
    {}
    
    public class RemoveUnifiedGroup : PSCmdlet
    {}
    
    public class RemoveUnifiedGroupLinks : PSCmdlet
    {}
    
    public class RemoveUserPhoto : PSCmdlet
    {}
    
    public class SetAccessToCustomerDataRequest : PSCmdlet
    {}
    
    public class SetAddressBookPolicy : PSCmdlet
    {}
    
    public class SetAddressList : PSCmdlet
    {}
    
    public class SetAuthenticationPolicy : PSCmdlet
    {}
    
    public class SetContact : PSCmdlet
    {}
    
    public class SetEmailAddressPolicy : PSCmdlet
    {}
    
    public class SetEOPDistributionGroup : PSCmdlet
    {}
    
    public class SetEOPGroup : PSCmdlet
    {}
    
    public class SetEOPMailUser : PSCmdlet
    {}
    
    public class SetEOPUser : PSCmdlet
    {}
    
    public class SetEventsFromEmailConfiguration : PSCmdlet
    {}
    
    public class SetGlobalAddressList : PSCmdlet
    {}
    
    public class SetLinkedUser : PSCmdlet
    {}
    
    public class SetMailbox : PSCmdlet
    {}
    
    public class SetMailboxAutoReplyConfiguration : PSCmdlet
    {}
    
    public class SetMailboxCalendarConfiguration : PSCmdlet
    {}
    
    public class SetMailboxJunkEmailConfiguration : PSCmdlet
    {}
    
    public class SetMailboxMessageConfiguration : PSCmdlet
    {}
    
    public class SetMailboxRegionalConfiguration : PSCmdlet
    {}
    
    public class SetMailboxSpellingConfiguration : PSCmdlet
    {}
    
    public class SetMailContact : PSCmdlet
    {}
    
    public class SetMailUser : PSCmdlet
    {}
    
    public class SetManagementRoleAssignment : PSCmdlet
    {}
    
    public class SetManagementRoleEntry : PSCmdlet
    {}
    
    public class SetManagementScope : PSCmdlet
    {}
    
    public class SetOfflineAddressBook : PSCmdlet
    {}
    
    public class SetOnPremisesOrganization : PSCmdlet
    {}
    
    public class SetPartnerApplication : PSCmdlet
    {}
    
    public class SetRoleAssignmentPolicy : PSCmdlet
    {}
    
    public class SetRoleGroup : PSCmdlet
    {}
    
    public class SetSiteMailbox : PSCmdlet
    {}
    
    public class SetTenantAnalyticsConfig : PSCmdlet
    {}
    
    public class SetUnifiedGroup : PSCmdlet
    {}
    
    public class SetUser : PSCmdlet
    {}
    
    public class SetUserAnalyticsConfig : PSCmdlet
    {}
    
    public class SetUserPhoto : PSCmdlet
    {}
    
    public class TestApplicationAccessPolicy : PSCmdlet
    {}
    
    public class TestSiteMailbox : PSCmdlet
    {}
    
    public class UndoSoftDeletedMailbox : PSCmdlet
    {}
    
    public class UndoSoftDeletedUnifiedGroup : PSCmdlet
    {}
    
    public class UpdateEOPDistributionGroupMember : PSCmdlet
    {}
    
    public class UpdateHybridConfiguration : PSCmdlet
    {}
    
    public class UpdateRecipient : PSCmdlet
    {}
    
    public class UpdateRoleGroupMember : PSCmdlet
    {}
    
    public class UpdateSiteMailbox : PSCmdlet
    {}
    
}