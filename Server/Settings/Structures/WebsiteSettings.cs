using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class WebsiteSettings : SettingsBase<WebsiteSettingsDefinition>
    {
        protected override string Filename => "WebsiteSettings.xml";
    }
}
