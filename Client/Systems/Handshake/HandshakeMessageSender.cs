using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeMessageSender : SubSystem<HandshakeSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<HandshakeCliMsg>(msg)));
        }

        public void SendHandshakeResponse(byte[] signature)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<HandshakeResponseMsgData>();
            msgData.PlayerName = SettingsSystem.CurrentSettings.PlayerName;
            msgData.ChallengeSignature = signature;
            msgData.PublicKey = SettingsSystem.CurrentSettings.PublicKey;

            SendMessage(msgData);
        }
    }
}