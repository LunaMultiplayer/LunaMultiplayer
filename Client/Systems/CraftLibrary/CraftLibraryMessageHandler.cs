using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryMessageHandler : SubSystem<CraftLibrarySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is CraftLibraryBaseMsgData msgData)) return;

            switch (msgData.CraftMessageType)
            {
                case CraftMessageType.ListReply:
                    {
                        var data = (CraftLibraryListReplyMsgData)msgData;
                        for (var i = 0; i < data.PlayerCraftsCount; i++)
                        {
                            if (data.PlayerCrafts[i].Crafts.VabExists)
                            {
                                for (var j = 0; j < data.PlayerCrafts[i].Crafts.VabCraftCount; j++)
                                {
                                    System.QueueCraftAdd(new CraftChangeEntry
                                    {
                                        PlayerName = data.PlayerCrafts[i].PlayerName,
                                        CraftType = CraftType.Vab,
                                        CraftName = data.PlayerCrafts[i].Crafts.VabCraftNames[j]
                                    });
                                }
                            }

                            if (data.PlayerCrafts[i].Crafts.SphExists)
                            {
                                for (var j = 0; j < data.PlayerCrafts[i].Crafts.SphCraftCount; j++)
                                {
                                    System.QueueCraftAdd(new CraftChangeEntry
                                    {
                                        PlayerName = data.PlayerCrafts[i].PlayerName,
                                        CraftType = CraftType.Sph,
                                        CraftName = data.PlayerCrafts[i].Crafts.SphCraftNames[j]
                                    });
                                }
                            }

                            if (data.PlayerCrafts[i].Crafts.SubassemblyExists)
                            {
                                for (var j = 0; j < data.PlayerCrafts[i].Crafts.SubassemblyCraftCount; j++)
                                {
                                    System.QueueCraftAdd(new CraftChangeEntry
                                    {
                                        PlayerName = data.PlayerCrafts[i].PlayerName,
                                        CraftType = CraftType.Subassembly,
                                        CraftName = data.PlayerCrafts[i].Crafts.SubassemblyCraftNames[j]
                                    });
                                }
                            }
                        }
                        MainSystem.NetworkState = ClientState.CraftlibrarySynced;
                    }
                    break;
                case CraftMessageType.AddFile:
                    {
                        var data = (CraftLibraryAddMsgData)msgData;
                        var cce = new CraftChangeEntry
                        {
                            PlayerName = data.PlayerName,
                            CraftType = data.UploadType,
                            CraftName = data.UploadName
                        };
                        System.QueueCraftAdd(cce);
                        ChatSystem.Singleton.PrintToChat($"{cce.PlayerName} shared {cce.CraftName} ({cce.CraftType})");
                    }
                    break;
                case CraftMessageType.DeleteFile:
                    {
                        var data = (CraftLibraryDeleteMsgData)msgData;
                        var cce = new CraftChangeEntry
                        {
                            PlayerName = data.PlayerName,
                            CraftType = data.CraftType,
                            CraftName = data.CraftName
                        };
                        System.QueueCraftDelete(cce);
                    }
                    break;
                case CraftMessageType.RespondFile:
                    {
                        var data = (CraftLibraryRespondMsgData)msgData;
                        var craftData = Common.TrimArray(data.CraftData, data.NumBytes);

                        var cre = new CraftResponseEntry
                        {
                            PlayerName = data.PlayerName,
                            CraftType = data.RequestedType,
                            CraftName = data.RequestedName,
                            CraftData = craftData
                        };

                        System.QueueCraftResponse(cre);
                    }
                    break;
            }
        }
    }
}