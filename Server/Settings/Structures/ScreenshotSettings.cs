using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class ScreenshotSettings : SettingsBase<ScreenshotSettingsDefinition>
    {
        protected override string Filename => "ScreenshotSettings.xml";
    }
}
