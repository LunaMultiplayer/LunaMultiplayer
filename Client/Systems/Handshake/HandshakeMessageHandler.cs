using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Mod;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaCommon.ModFile;
using System;
using System.Collections.Concurrent;

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
            
            switch (data.Response)
            {
                case HandshakeReply.HandshookSuccessfully:
                    ModSystem.Singleton.Clear();
                    ModSystem.Singleton.ModControl = data.ModControl;
                    if (ModSystem.Singleton.ModControl)
                    {
                        if (ModSystem.Singleton.ModFileHandler.ParseModFile(ModFileParser.ReadModFileFromString(data.ModFileData)))
                        {
                            LunaLog.Log("[LMP]: Handshake successful");
                            MainSystem.NetworkState = ClientState.Handshaked;
                        }
                        else
                        {
                            LunaLog.LogError("[LMP]: Failed to pass mod validation");
                            NetworkConnection.Disconnect("[LMP]: Failed mod validation");
                        }
                    }
                    else
                    {
                        LunaLog.Log("[LMP]: Handshake successful");
                        MainSystem.NetworkState = ClientState.Handshaked;
                    }
                    break;
                default:
                    var disconnectReason = $"Handshake failure: {data.Reason}";
                    LunaLog.Log(disconnectReason);
                    NetworkConnection.Disconnect(disconnectReason);
                    break;
            }
        }

        #endregion
    }
}
