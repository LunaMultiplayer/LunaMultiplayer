using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using System.Collections.Generic;
using System.IO;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryEvents : SubSystem<CraftLibrarySystem>
    {
        private bool DisplayCraftUploadingMessage { get; set; }

        public void HandleCraftLibraryEvents()
        {
            while (System.CraftAddQueue.Count > 0)
            {
                if (System.CraftAddQueue.TryDequeue(out var craftChangeEntry))
                    AddCraftEntry(craftChangeEntry.PlayerName, craftChangeEntry.CraftType, craftChangeEntry.CraftName);
            }

            while (System.CraftDeleteQueue.Count > 0)
            {
                if (System.CraftDeleteQueue.TryDequeue(out var craftDeleteEntry))
                    DeleteCraftEntry(craftDeleteEntry.PlayerName, craftDeleteEntry.CraftType, craftDeleteEntry.CraftName);
            }

            while (System.CraftResponseQueue.Count > 0)
            {
                if (System.CraftResponseQueue.TryDequeue(out var cre))
                    SaveCraftFile(cre.CraftType, cre.CraftName, cre.CraftData);
            }

            if (!string.IsNullOrEmpty(System.UploadCraftName))
            {
                UploadCraftFile(System.UploadCraftType, System.UploadCraftName);
                System.UploadCraftName = null;
                System.UploadCraftType = CraftType.Vab;
            }

            if (!string.IsNullOrEmpty(System.DownloadCraftName))
            {
                DownloadCraftFile(System.SelectedPlayer, System.DownloadCraftType, System.DownloadCraftName);
                System.DownloadCraftName = null;
                System.DownloadCraftType = CraftType.Vab;
            }

            if (!string.IsNullOrEmpty(System.DeleteCraftName))
            {
                DeleteCraftEntry(SettingsSystem.CurrentSettings.PlayerName, System.DeleteCraftType, System.DeleteCraftName);

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryDeleteMsgData>();
                msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
                msgData.CraftType = System.DeleteCraftType;
                msgData.CraftName = System.DeleteCraftName;

                System.MessageSender.SendMessage(msgData);
                System.DeleteCraftName = null;
                System.DeleteCraftType = CraftType.Vab;
            }

            if (DisplayCraftUploadingMessage)
            {
                ScreenMessages.PostScreenMessage("Craft uploaded!", 2f, ScreenMessageStyle.UPPER_CENTER);
                DisplayCraftUploadingMessage = false;
            }
        }

        private void UploadCraftFile(CraftType type, string name)
        {
            var uploadPath = "";
            switch (System.UploadCraftType)
            {
                case CraftType.Vab:
                    uploadPath = System.VabPath;
                    break;
                case CraftType.Sph:
                    uploadPath = System.SphPath;
                    break;
                case CraftType.Subassembly:
                    uploadPath = System.SubassemblyPath;
                    break;
            }
            var filePath = CommonUtil.CombinePaths(uploadPath, $"{name}.craft");
            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryUploadMsgData>();
                msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
                msgData.UploadType = type;
                msgData.UploadName = name;
                msgData.CraftData = fileData;
                msgData.NumBytes = fileData.Length;

                System.MessageSender.SendMessage(msgData);
                AddCraftEntry(SettingsSystem.CurrentSettings.PlayerName, System.UploadCraftType, System.UploadCraftName);
                DisplayCraftUploadingMessage = true;
            }
            else
            {
                LunaLog.LogError($"[LMP]: Cannot upload file, {filePath} does not exist!");
            }
        }

        private static void DownloadCraftFile(string playerName, CraftType craftType, string craftName)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<CraftLibraryRequestMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.RequestedType = craftType;
            msgData.CraftOwner = playerName;
            msgData.RequestedName = craftName;

            System.MessageSender.SendMessage(msgData);
        }

        private static void AddCraftEntry(string playerName, CraftType craftType, string craftName)
        {
            if (!System.PlayersWithCrafts.Contains(playerName))
                System.PlayersWithCrafts.Add(playerName);
            if (!System.PlayerList.ContainsKey(playerName))
                System.PlayerList.Add(playerName, new Dictionary<CraftType, List<string>>());
            if (!System.PlayerList[playerName].ContainsKey(craftType))
                System.PlayerList[playerName].Add(craftType, new List<string>());
            if (!System.PlayerList[playerName][craftType].Contains(craftName))
            {
                LunaLog.Log($"[LMP]: Adding {craftName}, type: {craftType} from {playerName}");
                System.PlayerList[playerName][craftType].Add(craftName);
            }
        }

        private static void DeleteCraftEntry(string playerName, CraftType craftType, string craftName)
        {
            if (System.PlayerList.ContainsKey(playerName) &&
                System.PlayerList[playerName].ContainsKey(craftType) &&
                System.PlayerList[playerName][craftType].Contains(craftName))
            {
                System.PlayerList[playerName][craftType].Remove(craftName);
                if (System.PlayerList[playerName][craftType].Count == 0)
                    System.PlayerList[playerName].Remove(craftType);
                if (System.PlayerList[playerName].Count == 0 && playerName != SettingsSystem.CurrentSettings.PlayerName)
                {
                    System.PlayerList.Remove(playerName);
                    if (System.PlayersWithCrafts.Contains(playerName))
                        System.PlayersWithCrafts.Remove(playerName);
                }
            }
        }

        private static void SaveCraftFile(CraftType craftType, string craftName, byte[] craftData)
        {
            var savePath = "";
            switch (craftType)
            {
                case CraftType.Vab:
                    savePath = System.VabPath;
                    break;
                case CraftType.Sph:
                    savePath = System.SphPath;
                    break;
                case CraftType.Subassembly:
                    savePath = System.SubassemblyPath;
                    break;
            }

            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            var craftFile = CommonUtil.CombinePaths(savePath, $"{craftName}.craft");
            File.WriteAllBytes(craftFile, craftData);
            ScreenMessages.PostScreenMessage($"Craft {craftName} saved!", 5f, ScreenMessageStyle.UPPER_CENTER);
        }
    }
}