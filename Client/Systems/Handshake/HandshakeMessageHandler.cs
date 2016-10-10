using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Mod;
using LunaClient.Systems.Network;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeMessageHandler : SubSystem<HandshakeSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (messageData.GetType() == typeof(HandshakeChallengeMsgData))
                HandleChallengeReceivedMessage((HandshakeChallengeMsgData) messageData);
            else if (messageData.GetType() == typeof(HandshakeReplyMsgData))
                HandleHandshakeReplyReceivedMessage((HandshakeReplyMsgData) messageData);
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
                    MainSystem.Singleton.NetworkState = ClientState.HANDSHAKING;
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error handling HANDSHAKE_CHALLANGE Message, exception: " + e);
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
                if (reply == HandshakeReply.HANDSHOOK_SUCCESSFULLY)
                {
                    ModSystem.Singleton.ModControl = data.ModControlMode;
                    if (ModSystem.Singleton.ModControl != ModControlMode.DISABLED)
                        modFileData = Encoding.UTF8.GetString(data.ModFileData);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error handling HANDSHAKE_REPLY Message, exception: " + e);
                reply = HandshakeReply.MALFORMED_HANDSHAKE;
                reason = "Incompatible HANDSHAKE_REPLY Message";
            }

            switch (reply)
            {
                case HandshakeReply.HANDSHOOK_SUCCESSFULLY:
                {
                    if (ModFileParser.ParseModFile(modFileData))
                    {
                        Debug.Log("Handshake successful");
                        MainSystem.Singleton.NetworkState = ClientState.AUTHENTICATED;
                    }
                    else
                    {
                        Debug.LogError("Failed to pass mod validation");
                        NetworkConnection.Disconnect("Failed mod validation");
                    }
                }
                    break;
                default:
                    var disconnectReason = "Handshake failure: " + reason;
                    //If it's a protocol mismatch, append the client/server version.
                    if (reply == HandshakeReply.PROTOCOL_MISMATCH)
                    {
                        disconnectReason += "\nClient: " + VersionInfo.VersionNumber + ", Server: " + data.Version;
                    }
                    Debug.Log(disconnectReason);
                    NetworkConnection.Disconnect(disconnectReason);
                    break;
            }
        }

        #endregion
    }
}