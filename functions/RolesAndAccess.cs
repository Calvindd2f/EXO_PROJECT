using System;
using Exchange.ConstantFunctions;
using Exchange.GlobalVariables;
using Exchange.Constants;
using System.Collections.Generic;
using System.Management.Automation;

namespace Exchange.functions.RolesAndAccess
{
    [Cmdlet(VerbsCommon.Add, 'MailboxLocation')]
    public class AddMailboxLocation : PSCmdlet
    { }
}