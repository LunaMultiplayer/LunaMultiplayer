using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class InterpolationSettings : SettingsBase<InterpolationSettingsDefinition>
    {
        protected override string Filename => "InterpolationSettings.xml";
    }
}
