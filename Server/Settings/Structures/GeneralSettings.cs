using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class GeneralSettings : SettingsBase<GeneralSettingsDefinition>
    {
        protected override string Filename => "GeneralSettings.xml";
    }
}
