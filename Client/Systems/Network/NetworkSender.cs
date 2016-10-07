using System;
using System.Collections.Concurrent;
using System.Threading;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Network
{
    public partial class NetworkSystem
    {
        private ConcurrentQueue<IClientMessageBase> OutgoingMessages { get; set; } = new ConcurrentQueue<IClientMessageBase>();
        public NetworkSimpleMessageSender SimpleMessageSender { get; } = new NetworkSimpleMessageSender();

        public void SendThreadMain()
        {
            try
            {
                while (ClientConnection != null && MainSystem.Singleton.NetworkState >= ClientState.CONNECTED)
                {
                    IClientMessageBase sendMessage;
                    if (OutgoingMessages.TryDequeue(out sendMessage))
                    {
                        SendNetworkMessage(sendMessage);
                    }
                    else
                    {
                        MainSystem.Delay(Common.SENDRECEIVE_INTERVAL);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.Debug("Send thread error: " + e);
            }
        }

        public void QueueOutgoingMessage(IClientMessageBase message)
        {
            OutgoingMessages.Enqueue(message);
        }

        private void SendNetworkMessage(IClientMessageBase message)
        {
            if (message.MessageType == ClientMessageType.SYNC_TIME)
                TimeSyncerSystem.Singleton.RewriteMessage(message.Data);

            var bytes = message.Serialize(SettingsSystem.CurrentSettings.CompressionEnabled);
            if (bytes != null)
            {
                try
                {
                    LastSendTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                    var lidgrenMsg = ClientConnection.CreateMessage(bytes.Length);
                    lidgrenMsg.Write(message.Serialize(SettingsSystem.CurrentSettings.CompressionEnabled));

                    ClientConnection.SendMessage(lidgrenMsg, message.NetDeliveryMethod, message.Channel);
                    ClientConnection.FlushSendQueue();
                }
                catch (Exception e)
                {
                    HandleDisconnectException(e);
                }
            }
        }
    }
}