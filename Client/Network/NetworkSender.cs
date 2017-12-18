using Lidgren.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Network
{
    public class NetworkSender
    {
        public static ConcurrentQueue<IMessageBase> OutgoingMessages { get; set; } = new ConcurrentQueue<IMessageBase>();
        public static NetworkSimpleMessageSender SimpleMessageSender { get; } = new NetworkSimpleMessageSender();

        /// <summary>
        /// Main sending thread
        /// </summary>
        public static void SendMain()
        {
            try
            {
                while (!NetworkConnection.ResetRequested)
                {
                    if (OutgoingMessages.TryDequeue(out var sendMessage))
                    {
                        SendNetworkMessage(sendMessage);
                    }
                    else
                    {
                        LunaDelay.Delay(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Send thread error: {e}");
            }
        }

        /// <summary>
        /// Adds a new message to the queue
        /// </summary>
        /// <param name="message"></param>
        public static void QueueOutgoingMessage(IMessageBase message)
        {
            OutgoingMessages.Enqueue(message);
        }

        /// <summary>
        /// Sends the network message. It will skip client messages to send when we are not connected
        /// </summary>
        /// <param name="message"></param>
        private static void SendNetworkMessage(IMessageBase message)
        {
            if (NetworkMain.ClientConnection.Status == NetPeerStatus.NotRunning)
                NetworkMain.ClientConnection.Start();

            message.Data.SentTime = LunaTime.UtcNow.Ticks;
            try
            {
                NetworkStatistics.LastSendTime = LunaTime.UtcNow;

                if (message is IMasterServerMessageBase)
                {
                    foreach (var masterServer in NetworkServerList.MasterServers)
                    {
                        //Don't reuse lidgren messages, he does that on it's own
                        var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage();

                        message.Serialize(lidgrenMsg, SettingsSystem.CurrentSettings.CompressionEnabled);
                        NetworkMain.ClientConnection.SendUnconnectedMessage(lidgrenMsg, masterServer);
                        NetworkMain.ClientConnection.FlushSendQueue();
                    }
                }
                else
                {
                    if (MainSystem.NetworkState >= ClientState.Connected)
                    {
                        var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage();

                        message.Serialize(lidgrenMsg, SettingsSystem.CurrentSettings.CompressionEnabled);
                        NetworkMain.ClientConnection.SendMessage(lidgrenMsg, message.NetDeliveryMethod, message.Channel);
                    }
                }

                NetworkMain.ClientConnection.FlushSendQueue();
            }
            catch (Exception e)
            {
                NetworkMain.HandleDisconnectException(e);
            }
            
            message.Recycle();
        }
    }
}