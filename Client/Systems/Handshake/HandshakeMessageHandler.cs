using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Mod;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeMessageHandler : SubSystem<HandshakeSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (messageData.GetType() == typeof(HandshakeChallengeMsgData))
                HandleChallengeReceivedMessage((HandshakeChallengeMsgData)messageData);
            else if (messageData.GetType() == typeof(HandshakeReplyMsgData))
                HandleHandshakeReplyReceivedMessage((HandshakeReplyMsgData)messageData);
        }

        #region Private

        public void HandleChallengeReceivedMessage(HandshakeChallengeMsgData messageData)
        {
            try
            {
                var challange = messageData.Challenge;
                using (var rsa = new RSACryptoServiceProvider(1024))
                {
                    rsa.PersistKeyInCsp = false;
                    rsa.FromXmlString(SettingsSystem.CurrentSettings.PrivateKey);
                    var signature = rsa.SignData(challange, CryptoConfig.CreateFromName("SHA256"));
                    System.MessageSender.SendHandshakeResponse(signature);
                    SystemsContainer.Get<MainSystem>().NetworkState = ClientState.Handshaking;
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error handling HANDSHAKE_CHALLANGE Message, exception: {e}");
            }
        }

        public void HandleHandshakeReplyReceivedMessage(HandshakeReplyMsgData data)
        {
            HandshakeReply reply;
            string reason;
            var modFileData = "";
            try
            {
                reply = data.Response;
                reason = data.Reason;
                //If we handshook successfully, the mod data will be available to read.
                if (reply == HandshakeReply.HandshookSuccessfully)
                {
                    SystemsContainer.Get<ModSystem>().ModControl = data.ModControlMode;
                    if (SystemsContainer.Get<ModSystem>().ModControl != ModControlMode.Disabled)
                        modFileData = Encoding.UTF8.GetString(data.ModFileData);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error handling HANDSHAKE_REPLY Message, exception: {e}");
                reply = HandshakeReply.MalformedHandshake;
                reason = "Incompatible HANDSHAKE_REPLY Message";
            }

            switch (reply)
            {
                case HandshakeReply.HandshookSuccessfully:
                    {
                        if (ModFileParser.ParseModFile(modFileData))
                        {
                            LunaLog.Log("[LMP]: Handshake successful");
                            SystemsContainer.Get<MainSystem>().NetworkState = ClientState.Authenticated;
                        }
                        else
                        {
                            LunaLog.LogError("[LMP]: Failed to pass mod validation");
                            NetworkConnection.Disconnect("[LMP]: Failed mod validation");
                        }
                    }
                    break;
                default:
                    var disconnectReason = $"Handshake failure: {reason}";
                    //If it's a protocol mismatch, append the client/server version.
                    if (reply == HandshakeReply.ProtocolMismatch)
                    {
                        disconnectReason += $"\nClient: {VersionInfo.VersionNumber}, Server: {data.Version}";
                    }
                    LunaLog.Log(disconnectReason);
                    NetworkConnection.Disconnect(disconnectReason);
                    break;
            }
        }

        #endregion
    }
}