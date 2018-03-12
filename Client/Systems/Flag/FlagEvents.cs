using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon;
using System.IO;

namespace LunaClient.Systems.Flag
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

            //If it's not a LMP flag don't sync it.
            if (!flagUrl.ToLower().StartsWith("lunamultiplayer/flags/")) return;

            var flagName = flagUrl.Substring("LunaMultiplayer/Flags/".Length) + ".png";
            var fullFlagPath = CommonUtil.CombinePaths(FlagSystem.FlagPath, flagName);

            //If the flag is owned by someone else don't sync it
            if (System.ServerFlags.TryGetValue(flagName, out var existingFlag) && existingFlag.Owner != SettingsSystem.CurrentSettings.PlayerName)
                return;
            
            if (File.Exists(fullFlagPath))
            {
                //Don't send the flag when the SHA sum already matches as that would mean that the server already has it
                if (existingFlag != null && existingFlag.ShaSum == Common.CalculateSha256FileHash(fullFlagPath)) return;

                LunaLog.Log($"[LMP]: Uploading {Path.GetFileName(fullFlagPath)}");
                
                System.MessageSender.SendMessage(System.MessageSender.GetFlagMessageData(flagName, fullFlagPath));
            }
        }
    }
}