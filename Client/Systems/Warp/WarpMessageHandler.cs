using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Warp;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaClient.Systems.Warp
{
    public class WarpMessageHandler : SubSystem<WarpSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as WarpBaseMsgData;
            if (msgData == null) return;

            switch (msgData.WarpMessageType)
            {
                case WarpMessageType.SUBSPACES_REPLY:
                    {
                        var data = (WarpSubspacesReplyMsgData)messageData;
                        for (var i = 0; i < data.SubspaceKey.Length; i++)
                        {
                            AddSubspace(data.SubspaceKey[i], data.SubspaceTime[i]);
                        }
                        foreach (var ps in data.Players)
                        {
                            if (System.ClientSubspaceList.ContainsKey(ps.Value))
                            {
                                System.ClientSubspaceList[ps.Value] = ps.Key;
                            }
                            else
                            {
                                System.ClientSubspaceList.Add(ps.Value, ps.Key);
                            }
                        }
                        
                        AddSubspace(0, 0); //Add server subspace
                        AddSubspace(-1, 0);//Add warping subspace

                        //Set us into server subspace
                        System.CurrentSubspace = 0;

                        MainSystem.Singleton.NetworkState = ClientState.WARPSUBSPACES_SYNCED;
                    }
                    break;
                case WarpMessageType.NEW_SUBSPACE:
                    {
                        var data = (WarpNewSubspaceMsgData)messageData;
                        AddSubspace(data.SubspaceKey, data.SubspaceTime);
                        if (data.PlayerCreator == SettingsSystem.CurrentSettings.PlayerName)
                        {
                            //It's our subspace that we just created so set it as ours
                            System.SkipSubspaceProcess = true;
                            System.CurrentSubspace = data.SubspaceKey;
                        }
                    }
                    break;
                case WarpMessageType.CHANGE_SUBSPACE:
                    {
                        var data = (WarpChangeSubspaceMsgData)messageData;
                        System.ClientSubspaceList[data.PlayerName] = data.Subspace;
                        VesselWarpSystem.Singleton.MovePlayerVesselsToNewSubspace(data.PlayerName, data.Subspace);
                    }
                    break;
                default:
                    {
                        LunaLog.Debug("Unhandled WARP_MESSAGE type: " + ((WarpBaseMsgData)messageData).WarpMessageType);
                        break;
                    }
            }
        }

        private static void AddSubspace(int subspaceId, double subspaceTime)
        {
            if (!System.Subspaces.ContainsKey(subspaceId))
                System.Subspaces.Add(subspaceId, subspaceTime);
        }
    }
}