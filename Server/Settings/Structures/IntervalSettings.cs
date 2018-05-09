using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class IntervalSettings : SettingsBase<IntervalSettingsDefinition>
    {
        protected override string Filename => "IntervalSettings.xml";
    }
}
