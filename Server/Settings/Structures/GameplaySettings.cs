using LmpCommon.Enums;
using LmpCommon.Xml;
using Server.Context;
using Server.Settings.Base;
using Server.Settings.Definition;
using System.IO;

namespace Server.Settings.Structures
{
    public class GameplaySettings : SettingsBase<GameplaySettingsDefinition>
    {
        protected override string Filename => "GameplaySettings.xml";

        public override void Load()
        {
            if (!File.Exists(SettingsPath))
            {
                var definition = new GameplaySettingsDefinition();
                switch (GeneralSettings.SettingsStore.GameDifficulty)
                {
                    case GameDifficulty.Easy:
                        definition.SetEasy();
                        break;
                    case GameDifficulty.Normal:
                        definition.SetNormal();
                        break;
                    case GameDifficulty.Moderate:
                        definition.SetModerate();
                        break;
                    case GameDifficulty.Hard:
                        definition.SetHard();
                        break;
                    case GameDifficulty.Custom:
                        definition.SetNormal();
                        break;
                }

                LunaXmlSerializer.WriteToXmlFile(definition, Path.Combine(ServerContext.ConfigDirectory, Filename));
            }

            base.Load();
        }
    }
}
