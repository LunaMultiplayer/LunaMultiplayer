using System;
using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.CraftLibrary;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;

namespace LmpClient.Systems.CraftLibrary
{
    public class CraftLibraryMessageHandler : SubSystem<CraftLibrarySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is CraftLibraryBaseMsgData msgData)) return;

            switch (msgData.CraftMessageType)
            {
                case CraftMessageType.FoldersReply:
                    var foldersMsg = (CraftLibraryFoldersReplyMsgData)msgData;
                    HandleCraftFolders(foldersMsg);
                    break;
                case CraftMessageType.ListReply:
                    var listMsg = (CraftLibraryListReplyMsgData)msgData;
                    HandleCraftList(listMsg);
                    break;
                case CraftMessageType.DeleteRequest:
                    var deleteMsg = (CraftLibraryDeleteRequestMsgData)msgData;
                    DeleteCraft(deleteMsg);
                    break;
                case CraftMessageType.CraftData:
                    var craftMsg = (CraftLibraryDataMsgData)msgData;
                    SaveNewCraft(craftMsg);
                    break;
                case CraftMessageType.Notification:
                    var notificationMsg = (CraftLibraryNotificationMsgData)msgData;
                    System.FoldersWithNewContent.Add(notificationMsg.FolderName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void HandleCraftFolders(CraftLibraryFoldersReplyMsgData foldersMsg)
        {
            System.CraftInfo.Clear();
            for (var i = 0; i < foldersMsg.NumFolders; i++)
            {
                System.CraftInfo.TryAdd(foldersMsg.Folders[i], new ConcurrentDictionary<string, CraftBasicEntry>());
                System.CraftDownloaded.TryAdd(foldersMsg.Folders[i], new ConcurrentDictionary<string, CraftEntry>());
            }
        }

        private static void HandleCraftList(CraftLibraryListReplyMsgData listMsg)
        {
            if (System.CraftInfo.TryGetValue(listMsg.FolderName, out var craftEntries))
            {
                for (var i = 0; i < listMsg.PlayerCraftsCount; i++)
                {
                    var craftInfo = new CraftBasicEntry
                    {
                        CraftName = listMsg.PlayerCrafts[i].CraftName,
                        CraftType = listMsg.PlayerCrafts[i].CraftType,
                        FolderName = listMsg.PlayerCrafts[i].FolderName
                    };
                    craftEntries.AddOrUpdate(listMsg.PlayerCrafts[i].CraftName, craftInfo, (key, existingVal) => craftInfo);
                }
            }
        }

        private static void DeleteCraft(CraftLibraryDeleteRequestMsgData deleteMsg)
        {
            if (System.CraftInfo.TryGetValue(deleteMsg.CraftToDelete.FolderName, out var folderCraftEntries))
            {
                folderCraftEntries.TryRemove(deleteMsg.CraftToDelete.CraftName, out _);
                //No crafts in this folder so remove it
                if (folderCraftEntries.Count == 0)
                {
                    System.CraftInfo.TryRemove(deleteMsg.CraftToDelete.FolderName, out _);
                }
            }

            if (System.CraftDownloaded.TryGetValue(deleteMsg.CraftToDelete.FolderName, out var downloadedCrafts))
            {
                downloadedCrafts.TryRemove(deleteMsg.CraftToDelete.CraftName, out _);
            }
        }

        private static void SaveNewCraft(CraftLibraryDataMsgData craftMsg)
        {
            var craft = new CraftEntry
            {
                CraftName = craftMsg.Craft.CraftName,
                CraftType = craftMsg.Craft.CraftType,
                FolderName = craftMsg.Craft.FolderName,
                CraftNumBytes = craftMsg.Craft.NumBytes,
                CraftData = new byte[craftMsg.Craft.NumBytes]
            };

            Array.Copy(craftMsg.Craft.Data, craft.CraftData, craftMsg.Craft.NumBytes);

            if (System.CraftDownloaded.TryGetValue(craftMsg.Craft.FolderName, out var downloadedCrafts))
                downloadedCrafts.AddOrUpdate(craftMsg.Craft.CraftName, craft, (key, existingVal) => craft);

            System.SaveCraftToDisk(craft);
        }
    }
}
