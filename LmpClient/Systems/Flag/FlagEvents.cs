using LmpClient.Base;
using LmpClient.Systems.SettingsSys;

namespace LmpClient.Systems.Flag
{
    public class FlagEvents : SubSystem<FlagSystem>
    {
        public void OnFlagSelect(string data)
        {
            HandleFlagChangeEvent(data);
        }

        public void OnMissionFlagSelect(string data)
        {
            HandleFlagChangeEvent(data);
        }

        /// <summary>
        /// Called when we change the flag of a mission or in general. 
        /// This will upload the flag to the server.
        /// </summary>
        private static void HandleFlagChangeEvent(string flagUrl)
        {
            SettingsSystem.CurrentSettings.SelectedFlag = flagUrl;
            SettingsSystem.SaveSettings();

            System.SendFlag(flagUrl);
        }
    }
}