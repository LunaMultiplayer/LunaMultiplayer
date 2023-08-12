using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class MetricsSettings : SettingsBase<MetricsSettingsDefinition>
    {
        protected override string Filename => "MetricsSettings.xml";
    }
}
