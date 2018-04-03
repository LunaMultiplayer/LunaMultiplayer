using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeMessageSender : SubSystem<HandshakeSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<HandshakeCliMsg>(msg)));
        }

        public void SendHandshakeRequest()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<HandshakeRequestMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.UniqueIdentifier = SystemInfo.deviceUniqueIdentifier;

            SendMessage(msgData);
        }
    }
}
