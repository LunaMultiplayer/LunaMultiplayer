using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Network;
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
            NetworkSystem.Singleton.QueueOutgoingMessage(MessageFactory.CreateNew<HandshakeCliMsg>(msg));
        }

        public void SendHandshakeResponse(byte[] signature)
        {
            var msgData = new HandshakeResponseMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                ChallengeSignature = signature,
                PublicKey = SettingsSystem.CurrentSettings.PublicKey
            };
            SendMessage(msgData);
        }
    }
}