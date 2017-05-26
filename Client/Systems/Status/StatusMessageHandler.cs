using System;
using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaClient.Systems.Status
{
    public class StatusMessageHandler : SubSystem<StatusSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as PlayerStatusBaseMsgData;
            if (msgData == null) return;

            switch (msgData.PlayerStatusMessageType)
            {
                case PlayerStatusMessageType.Reply:
                    HandlePlayerStatusReply(messageData);
                    break;
                case PlayerStatusMessageType.Set:
                    AddNewPlayerStatus(messageData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void HandlePlayerStatusReply(IMessageData messageData)
        {
            var msg = (PlayerStatusReplyMsgData)messageData;

            for (var i = 0; i < msg.PlayerName.Length; i++)
            {
                AddNewPlayerStatus(new PlayerStatusSetMsgData
                {
                    PlayerName = msg.PlayerName[i],
                    StatusText = msg.StatusText[i],
                    VesselText = msg.VesselText[i]
                });
            }
            MainSystem.Singleton.NetworkState = ClientState.PlayersSynced;
        }

        private static void AddNewPlayerStatus(IMessageData messageData)
        {
            var msg = (PlayerStatusSetMsgData) messageData;
            var newStatus = new PlayerStatus
            {
                PlayerName = msg.PlayerName,
                VesselText = msg.VesselText,
                StatusText = msg.StatusText
            };

            if (System.PlayerStatusList.ContainsKey(newStatus.PlayerName))
            {
                System.PlayerStatusList[newStatus.PlayerName].VesselText = newStatus.VesselText;
                System.PlayerStatusList[newStatus.PlayerName].StatusText = newStatus.StatusText;
            }
            else
            {
                System.PlayerStatusList.Add(newStatus.PlayerName, newStatus);
            }
        }
    }
}