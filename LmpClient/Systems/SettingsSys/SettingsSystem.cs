using LmpClient.Base;
using LmpClient.Network;
using LmpCommon.Enums;
using System.Text;

namespace LmpClient.Systems.SettingsSys
{
    public class SettingsSystem : MessageSystem<SettingsSystem, SettingsMessageSender, SettingsMessageHandler>
    {
        public static SettingStructure CurrentSettings { get; }
        public static SettingsServerStructure ServerSettings { get; private set; } = new SettingsServerStructure();

        private static readonly StringBuilder Builder = new StringBuilder();

        public override string SystemName { get; } = nameof(SettingsSystem);

        static SettingsSystem() => CurrentSettings = SettingsReadSaveHandler.ReadSettings();

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ServerSettings = new SettingsServerStructure();
        }

        public static void SaveSettings()
        {
            SettingsReadSaveHandler.SaveSettings(CurrentSettings);
        }

        public static bool ValidateSettings()
        {
            Builder.Length = 0;
            var validationResult = true;

            if (ServerSettings.TerrainQuality != TerrainQuality.Ignore && (int)ServerSettings.TerrainQuality != PQSCache.PresetList.presetIndex)
            {
                validationResult = false;
                Builder.Append($"Your terrain quality: {(TerrainQuality)PQSCache.PresetList.presetIndex} does not match the server quality: {ServerSettings.TerrainQuality}.");
            }

            if (!validationResult)
            {
                NetworkConnection.Disconnect(Builder.ToString());
            }

            return validationResult;
        }
    }
}
