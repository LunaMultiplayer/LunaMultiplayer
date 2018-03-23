using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using System;
using System.Security.Cryptography;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeSystem : MessageSystem<HandshakeSystem, HandshakeMessageSender, HandshakeMessageHandler>
    {
        public override string SystemName { get; } = nameof(HandshakeSystem);

        public byte[] Challenge = new byte[1024];

        protected override bool AlwaysEnabled => true;
        protected override bool ProcessMessagesInUnityThread => false;

        public void SendHandshakeChallengeResponse()
        {
            try
            {
                using (var rsa = new RSACryptoServiceProvider(1024))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.FromXmlString(SettingsSystem.CurrentSettings.PrivateKey);
                    var signature = rsa.SignData(Challenge, CryptoConfig.CreateFromName("SHA256"));
                    MessageSender.SendHandshakeResponse(signature);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error sending HandshakeResponseMsgData, exception: {e}");
            }
        }
    }
}
