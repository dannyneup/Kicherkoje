namespace Kicherkoje.Automations.Apps.Shared;

public class ConfigBase
{
    public ConfigBase()
    {
        if (HaContext != null)
            Entities = new Entities(HaContext);
    }

    public IHaContext HaContext { get; init; }

    protected Entities Entities { get; }
}