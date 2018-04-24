using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class WarpSettings : SettingsBase<WarpSettingsDefinition>
    {
        protected override string Filename => "WarpSettings.xml";
    }
}
