using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings
{
    public class CraftSettings : SettingsBase<CraftSettingsDefinition>
    {
        protected override string Filename => "CraftSettings.xml";
    }
}
