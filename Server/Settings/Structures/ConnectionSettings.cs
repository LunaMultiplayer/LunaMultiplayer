using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class ConnectionSettings : SettingsBase<ConnectionSettingsDefinition>
    {
        protected override string Filename => "ConnectionSettings.xml";
    }
}
