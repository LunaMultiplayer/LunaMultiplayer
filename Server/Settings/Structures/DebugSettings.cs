using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class DebugSettings : SettingsBase<DebugSettingsDefinition>
    {
        protected override string Filename => "DebugSettings.xml";
    }
}
