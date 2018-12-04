using ByteSizeLib;
using LmpCommon.Enums;
using LmpCommon.Message.Data.CraftLibrary;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings.Structures;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System
{
    public class CraftLibrarySystem
    {
        public static readonly string CraftPath = Path.Combine(ServerContext.UniverseDirectory, "Crafts");
        private static readonly ConcurrentDictionary<string, DateTime> LastRequest = new ConcurrentDictionary<string, DateTime>();

        #region Public Methods

        /// <summary>
        /// Deletes a requested craft
        /// </summary>
        public static void DeleteCraft(ClientStructure client, CraftLibraryDeleteRequestMsgData data)
        {
            if (client.PlayerName != data.CraftToDelete.FolderName)
                return;

            Task.Run(() =>
            {
                var file = Path.Combine(CraftPath, data.CraftToDelete.FolderName, data.CraftToDelete.CraftType.ToString(),
                    $"{data.CraftToDelete.CraftName}.craft");

                if (FileHandler.FileExists(file))
                {
                    FileHandler.FileDelete(file);

                    LunaLog.Debug($"Deleting craft {data.CraftToDelete.CraftName} as requested by {client.PlayerName}.");
                    MessageQueuer.SendToAllClients<CraftLibrarySrvMsg>(data);
                }
            });
        }

        /// <summary>
        /// Saves a received craft
        /// </summary>
        public static void SaveCraft(ClientStructure client, CraftLibraryDataMsgData data)
        {
            Task.Run(() =>
            {
                var playerFolderType = Path.Combine(CraftPath, client.PlayerName, data.Craft.CraftType.ToString());
                if (!Directory.Exists(playerFolderType))
                {
                    Directory.CreateDirectory(playerFolderType);
                }

                var lastTime = LastRequest.GetOrAdd(client.PlayerName, DateTime.MinValue);
                if (DateTime.Now - lastTime > TimeSpan.FromMilliseconds(CraftSettings.SettingsStore.MinCraftLibraryRequestIntervalMs))
                {
                    LastRequest.AddOrUpdate(client.PlayerName, DateTime.Now, (key, existingVal) => DateTime.Now);
                    var fileName = $"{data.Craft.CraftName}.craft";
                    var fullPath = Path.Combine(playerFolderType, fileName);

                    if (FileHandler.FileExists(fullPath))
                    {
                        LunaLog.Normal($"Overwriting craft {data.Craft.CraftName} ({ByteSize.FromBytes(data.Craft.NumBytes).KiloBytes}{ByteSize.KiloByteSymbol}) from: {client.PlayerName}.");

                        //Send a msg to all the players so they remove the old copy
                        var deleteMsg = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryDeleteRequestMsgData>();
                        deleteMsg.CraftToDelete.CraftType = data.Craft.CraftType;
                        deleteMsg.CraftToDelete.CraftName = data.Craft.CraftName;
                        deleteMsg.CraftToDelete.FolderName = data.Craft.FolderName;

                        MessageQueuer.SendToAllClients<CraftLibrarySrvMsg>(deleteMsg);
                    }
                    else
                    {
                        LunaLog.Normal($"Saving craft {data.Craft.CraftName} ({ByteSize.FromBytes(data.Craft.NumBytes).KiloBytes} KB) from: {client.PlayerName}.");
                        FileHandler.WriteToFile(fullPath, data.Craft.Data, data.Craft.NumBytes);
                    }
                    SendNotification(client.PlayerName);
                }
                else
                {
                    LunaLog.Warning($"{client.PlayerName} is sending crafts too fast!");
                    return;
                }

                //Remove oldest crafts if the player has too many
                RemovePlayerOldestCrafts(playerFolderType);

                //Checks if we are above the max folders limit
                CheckMaxFolders();
            });
        }
        
        /// <summary>
        /// Send the craft folders that exist on the server
        /// </summary>
        public static void SendCraftFolders(ClientStructure client)
        {
            Task.Run(() =>
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryFoldersReplyMsgData>();
                msgData.Folders = Directory.GetDirectories(CraftPath)
                    .Where(d=> Directory.GetFiles(d, "*.craft", SearchOption.AllDirectories).Length > 0)
                    .Select(d => new DirectoryInfo(d).Name).ToArray();

                msgData.NumFolders = msgData.Folders.Length;

                MessageQueuer.SendToClient<CraftLibrarySrvMsg>(client, msgData);
                if (msgData.NumFolders > 0)
                    LunaLog.Debug($"Sending {msgData.NumFolders} craft folders to: {client.PlayerName}");
            });
        }

        /// <summary>
        /// Sends the crafts in a folder
        /// </summary>
        public static void SendCraftList(ClientStructure client, CraftLibraryListRequestMsgData data)
        {
            Task.Run(() =>
            {
                var crafts = new List<CraftBasicInfo>();
                var playerFolder = Path.Combine(CraftPath, data.FolderName);

                foreach (var craftType in Enum.GetNames(typeof(CraftType)))
                {
                    var craftTypeFolder = Path.Combine(playerFolder, craftType);
                    if (Directory.Exists(craftTypeFolder))
                    {
                        foreach (var file in Directory.GetFiles(craftTypeFolder))
                        {
                            var craftName = Path.GetFileNameWithoutExtension(file);
                            crafts.Add(new CraftBasicInfo
                            {
                                CraftName = craftName,
                                CraftType = (CraftType)Enum.Parse(typeof(CraftType), craftType),
                                FolderName = data.FolderName
                            });
                        }
                    }
                }

                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryListReplyMsgData>();
                
                msgData.FolderName = data.FolderName;
                msgData.PlayerCrafts = crafts.ToArray();
                msgData.PlayerCraftsCount = crafts.Count;

                MessageQueuer.SendToClient<CraftLibrarySrvMsg>(client, msgData);
                if (msgData.PlayerCraftsCount > 0)
                    LunaLog.Debug($"Sending {msgData.PlayerCraftsCount} ({data.FolderName}) crafts to: {client.PlayerName}");
            });
        }

        /// <summary>
        /// Sends the requested craft
        /// </summary>
        public static void SendCraft(ClientStructure client, CraftLibraryDownloadRequestMsgData data)
        {
            Task.Run(() =>
            {
                var lastTime = LastRequest.GetOrAdd(client.PlayerName, DateTime.MinValue);
                if (DateTime.Now - lastTime > TimeSpan.FromMilliseconds(CraftSettings.SettingsStore.MinCraftLibraryRequestIntervalMs))
                {
                    LastRequest.AddOrUpdate(client.PlayerName, DateTime.Now, (key, existingVal) => DateTime.Now);
                    var file = Path.Combine(CraftPath, data.CraftRequested.FolderName, data.CraftRequested.CraftType.ToString(),
                        $"{data.CraftRequested.CraftName}.craft");

                    if (FileHandler.FileExists(file))
                    {
                        var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryDataMsgData>();
                        msgData.Craft.CraftType = data.CraftRequested.CraftType;
                        msgData.Craft.Data = FileHandler.ReadFile(file);
                        msgData.Craft.NumBytes = msgData.Craft.Data.Length;
                        msgData.Craft.FolderName = data.CraftRequested.FolderName;
                        msgData.Craft.CraftName = data.CraftRequested.CraftName;

                        LunaLog.Debug($"Sending craft ({ByteSize.FromBytes(msgData.Craft.NumBytes).KiloBytes}{ByteSize.KiloByteSymbol}): {data.CraftRequested.CraftName} to: {client.PlayerName}.");
                        MessageQueuer.SendToClient<CraftLibrarySrvMsg>(client, msgData);
                    }
                }
                else
                {
                    LunaLog.Warning($"{client.PlayerName} is requesting crafts too fast!");
                }
            });
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sends a notification of new craft to all players
        /// </summary>
        private static void SendNotification(string folderName)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<CraftLibraryNotificationMsgData>();
            msgData.FolderName = folderName;

            MessageQueuer.SendToAllClients<CraftLibrarySrvMsg>(msgData);
        }

        /// <summary>
        /// Checks if we have too many player folders and if so, it deletes the oldest one
        /// </summary>
        private static void CheckMaxFolders()
        {
            while (Directory.GetDirectories(CraftPath).Length > CraftSettings.SettingsStore.MaxCraftFolders)
            {
                var oldestFolder = Directory.GetDirectories(CraftPath).Select(d => new DirectoryInfo(d)).OrderBy(d => d.LastWriteTime).FirstOrDefault();
                if (oldestFolder != null)
                {
                    LunaLog.Debug($"Removing oldest crafts folder {oldestFolder.Name}");
                    Directory.Delete(oldestFolder.FullName, true);
                }
            }
        }

        /// <summary>
        /// If the player has too many crafts this method will remove the oldest ones
        /// </summary>
        private static void RemovePlayerOldestCrafts(string playerFolderType)
        {
            while (new DirectoryInfo(playerFolderType).GetFiles().Length > CraftSettings.SettingsStore.MaxCraftsPerUser)
            {
                var oldestCraft = new DirectoryInfo(playerFolderType).GetFiles().OrderBy(f => f.LastWriteTime).FirstOrDefault();
                if (oldestCraft != null)
                {
                    LunaLog.Debug($"Deleting old craft {oldestCraft.FullName}");
                    FileHandler.FileDelete(oldestCraft.FullName);
                }
            }
        }

        #endregion
    }
}
