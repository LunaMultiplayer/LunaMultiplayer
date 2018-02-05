using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Status
{
    public class StatusMessageHandler : SubSystem<StatusSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is PlayerStatusBaseMsgData msgData)) return;

            switch (msgData.PlayerStatusMessageType)
            {
                case PlayerStatusMessageType.Reply:
                    HandlePlayerStatusReply(msgData);
                    break;
                case PlayerStatusMessageType.Set:
                    var msgStatusData = (PlayerStatusSetMsgData)msgData;
                    AddNewPlayerStatus(msgStatusData.PlayerStatus.PlayerName, msgStatusData.PlayerStatus.VesselText, msgStatusData.PlayerStatus.StatusText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void HandlePlayerStatusReply(IMessageData messageData)
        {
            var msg = (PlayerStatusReplyMsgData)messageData;

            for (var i = 0; i < msg.PlayerStatusCount; i++)
            {
                AddNewPlayerStatus(msg.PlayerStatus[i].PlayerName, msg.PlayerStatus[i].VesselText, msg.PlayerStatus[i].StatusText);
            }

            MainSystem.NetworkState = ClientState.PlayersSynced;
        }

        private static void AddNewPlayerStatus(string playerName, string vesselText, string statusText)
        {
            if (System.PlayerStatusList.ContainsKey(playerName))
            {
                System.PlayerStatusList[playerName].VesselText = vesselText;
                System.PlayerStatusList[playerName].StatusText = statusText;
            }
            else
            {
                System.PlayerStatusList.TryAdd(playerName, new PlayerStatus
                {
                    PlayerName = playerName,
                    VesselText = vesselText,
                    StatusText = statusText
                });
            }
        }
    }
}