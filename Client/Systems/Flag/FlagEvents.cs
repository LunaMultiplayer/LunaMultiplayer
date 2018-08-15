using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using LunaCommon.Flags;
using System.IO;
using UniLinq;

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

            //If it's a default flag skip the sending
            if (DefaultFlags.DefaultFlagList.Contains(flagUrl))
                return;

            //If the flag is owned by someone else don't sync it
            if (System.ServerFlags.TryGetValue(flagUrl, out var existingFlag) && existingFlag.Owner != SettingsSystem.CurrentSettings.PlayerName)
                return;

            var textureInfo = GameDatabase.Instance.GetTextureInfo(flagUrl);
            if (textureInfo != null)
            {
                var flagData = textureInfo.normalMap.GetRawTextureData();
                if (flagData.Length > 1000000)
                {
                    LunaLog.LogError($"Cannot upload flag {Path.GetFileName(flagUrl)} size is greater than 1Mb!");
                    return;
                }

                //Don't send the flag when the SHA sum already matches as that would mean that the server already has it
                if (existingFlag != null && existingFlag.ShaSum == Common.CalculateSha256Hash(flagData)) return;

                LunaLog.Log($"[LMP]: Uploading {Path.GetFileName(flagUrl)} flag");
                
                System.MessageSender.SendMessage(System.MessageSender.GetFlagMessageData(flagUrl, flagData));
            }
        }
    }
}