using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Mod;
using LunaClient.Systems.TimeSyncer;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace LunaClient.Systems.Handshake
{
    public class HandshakeMessageHandler : SubSystem<HandshakeSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is HandshakeBaseMsgData msgData)) return;

            switch (msgData.HandshakeMessageType)
            {
                case HandshakeMessageType.Challenge:
                    Array.Copy(((HandshakeChallengeMsgData)msgData).Challenge, System.Challenge, 1024);
                    MainSystem.NetworkState = ClientState.HandshakeChallengeReceived;
                    break;
                case HandshakeMessageType.Reply:
                    HandleHandshakeReplyReceivedMessage((HandshakeReplyMsgData)msgData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Private

        public void HandleHandshakeReplyReceivedMessage(HandshakeReplyMsgData data)
        {
            TimeSyncerSystem.ServerStartTime = data.ServerStartTime;

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
                    ModSystem.Singleton.ModControl = data.ModControlMode;
                    if (ModSystem.Singleton.ModControl != ModControlMode.Disabled)
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

                    if (ModFileHandler.ParseModFile(modFileData))
                    {
                        LunaLog.Log("[LMP]: Handshake successful");
                        MainSystem.NetworkState = ClientState.Authenticated;
                    }
                    else
                    {
                        LunaLog.LogError("[LMP]: Failed to pass mod validation");
                        NetworkConnection.Disconnect("[LMP]: Failed mod validation");
                    }
                    break;
                default:
                    var disconnectReason = $"Handshake failure: {reason}";
                    //If it's a protocol mismatch, append the client/server version.
                    if (reply == HandshakeReply.ProtocolMismatch)
                    {
                        disconnectReason += $"\nClient: {LmpVersioning.CurrentVersion}, Server: {data.MajorVersion}.{data.MinorVersion}.{data.BuildVersion}";
                    }
                    LunaLog.Log(disconnectReason);
                    NetworkConnection.Disconnect(disconnectReason);
                    break;
            }
        }

        #endregion
    }
}