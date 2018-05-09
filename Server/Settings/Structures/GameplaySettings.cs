using Server.Settings.Base;
using Server.Settings.Definition;

namespace Server.Settings.Structures
{
    public class GameplaySettings : SettingsBase<GameplaySettingsDefinition>
    {
        protected override string Filename => "GameplaySettings.xml";
    }
}
