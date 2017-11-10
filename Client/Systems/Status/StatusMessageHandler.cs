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

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is PlayerStatusBaseMsgData msgData)) return;

            switch (msgData.PlayerStatusMessageType)
            {
                case PlayerStatusMessageType.Reply:
                    HandlePlayerStatusReply(messageData);
                    break;
                case PlayerStatusMessageType.Set:
                    var msg = (PlayerStatusSetMsgData)messageData;
                    AddNewPlayerStatus(msg.PlayerName, msg.VesselText, msg.StatusText);
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
                AddNewPlayerStatus(msg.PlayerName[i], msg.VesselText[i], msg.StatusText[i]);
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