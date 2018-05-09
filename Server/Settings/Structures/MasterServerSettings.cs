using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class MasterServerSettings : SettingsBase<MasterServerSettingsDefinition>
    {
        protected override string Filename => "MasterServerSettings.xml";
    }
}
