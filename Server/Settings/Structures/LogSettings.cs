using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class LogSettings : SettingsBase<LogSettingsDefinition>
    {
        protected override string Filename => "LogSettings.xml";
    }
}
