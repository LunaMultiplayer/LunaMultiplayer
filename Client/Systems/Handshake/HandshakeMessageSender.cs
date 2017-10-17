using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using System.Threading.Tasks;

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