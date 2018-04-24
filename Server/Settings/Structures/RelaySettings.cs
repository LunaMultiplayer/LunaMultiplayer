using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class RelaySettings : SettingsBase<RelaySettingsDefinition>
    {
        protected override string Filename => "RelaySettings.xml";
    }
}
