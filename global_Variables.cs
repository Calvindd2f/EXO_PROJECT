// region Global Variables

[Cmdlet(VerbsLifecycle.Should, "UseCustomRoutingByDefault")]
[OutputType(typeof(bool))]
public class ShouldUseCustomRoutingByDefaultCommand : Cmdlet
{
    protected override void ProcessRecord()
    {
        WriteObject(false);
    }
}

// endregion
