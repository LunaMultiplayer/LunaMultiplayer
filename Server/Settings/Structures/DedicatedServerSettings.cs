using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class DedicatedServerSettings : SettingsBase<DedicatedServerSettingsDefinition>
    {
        protected override string Filename => "DedicatedServerSettings.xml";
    }
}
