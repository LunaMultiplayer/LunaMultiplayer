using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Chat;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using LunaCommon.Message.Data.CraftLibrary;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryMessageHandler : SubSystem<CraftLibrarySystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as CraftLibraryBaseMsgData;
            if (msgData == null) return;

            switch (msgData.CraftMessageType)
            {
                case CraftMessageType.ListReply:
                    {
                        var data = (CraftLibraryListReplyMsgData)messageData;
                        var playerList = data.PlayerCrafts;
                        foreach (var playerCraft in playerList)
                        {
                            var vabExists = playerCraft.Value.VabExists;
                            var sphExists = playerCraft.Value.SphExists;
                            var subassemblyExists = playerCraft.Value.SubassemblyExists;
                            LunaLog.Log($"[LMP]: Player: {playerCraft.Key}, VAB: {vabExists}, SPH: {sphExists}, SUBASSEMBLY {subassemblyExists}");
                            if (vabExists)
                            {
                                var vabCrafts = playerCraft.Value.VabCraftNames;
                                foreach (var vabCraft in vabCrafts)
                                {
                                    var cce = new CraftChangeEntry
                                    {
                                        PlayerName = playerCraft.Key,
                                        CraftType = CraftType.Vab,
                                        CraftName = vabCraft
                                    };
                                    System.QueueCraftAdd(cce);
                                }
                            }
                            if (sphExists)
                            {
                                var sphCrafts = playerCraft.Value.SphCraftNames;
                                foreach (var sphCraft in sphCrafts)
                                {
                                    var cce = new CraftChangeEntry
                                    {
                                        PlayerName = playerCraft.Key,
                                        CraftType = CraftType.Sph,
                                        CraftName = sphCraft
                                    };
                                    System.QueueCraftAdd(cce);
                                }
                            }
                            if (subassemblyExists)
                            {
                                var subassemblyCrafts = playerCraft.Value.SubassemblyCraftNames;
                                foreach (var subassemblyCraft in subassemblyCrafts)
                                {
                                    var cce = new CraftChangeEntry
                                    {
                                        PlayerName = playerCraft.Key,
                                        CraftType = CraftType.Subassembly,
                                        CraftName = subassemblyCraft
                                    };
                                    System.QueueCraftAdd(cce);
                                }
                            }
                        }
                        SystemsContainer.Get<MainSystem>().NetworkState = ClientState.CraftlibrarySynced;
                    }
                    break;
                case CraftMessageType.AddFile:
                    {
                        var data = (CraftLibraryAddMsgData)messageData;
                        var cce = new CraftChangeEntry
                        {
                            PlayerName = data.PlayerName,
                            CraftType = data.UploadType,
                            CraftName = data.UploadName
                        };
                        System.QueueCraftAdd(cce);
                        SystemsContainer.Get<ChatSystem>().Queuer.QueueChannelMessage(SettingsSystem.ServerSettings.ConsoleIdentifier, "",
                            $"{cce.PlayerName} shared {cce.CraftName} ({cce.CraftType})");
                    }
                    break;
                case CraftMessageType.DeleteFile:
                    {
                        var data = (CraftLibraryDeleteMsgData)messageData;
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
                        var data = (CraftLibraryRespondMsgData)messageData;
                        var cre = new CraftResponseEntry
                        {
                            PlayerName = data.PlayerName,
                            CraftType = data.RequestedType,
                            CraftName = data.RequestedName
                        };
                        var hasCraft = data.HasCraft;
                        if (hasCraft)
                        {
                            cre.CraftData = data.CraftData;
                            System.QueueCraftResponse(cre);
                        }
                        else
                        {
                            ScreenMessages.PostScreenMessage(
                                $"Craft {cre.CraftName} from {cre.PlayerName} not available", 5f,
                                ScreenMessageStyle.UPPER_CENTER);
                        }
                    }
                    break;
            }
        }
    }
}