using Lidgren.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;
using LmpCommon.Message.Interface;
using LmpCommon.RepoRetrievers;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace LmpClient.Network
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
                    if (OutgoingMessages.Count > 0 && OutgoingMessages.TryDequeue(out var sendMessage))
                    {
                        SendNetworkMessage(sendMessage);
                    }
                    else
                    {
                        Thread.Sleep(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
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

            message.Data.SentTime = LunaNetworkTime.UtcNow.Ticks;
            try
            {
                NetworkStatistics.LastSendTime = LunaNetworkTime.UtcNow;

                if (message is IMasterServerMessageBase)
                {
                    foreach (var masterServer in MasterServerRetriever.MasterServers.GetValues)
                    {
                        //Don't reuse lidgren messages, he does that on it's own
                        var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage(message.GetMessageSize());

                        message.Serialize(lidgrenMsg);
                        NetworkMain.ClientConnection.SendUnconnectedMessage(lidgrenMsg, masterServer);

                        //Force send of packets
                        NetworkMain.ClientConnection.FlushSendQueue();
                    }
                }
                else
                {
                    if (MainSystem.NetworkState >= ClientState.Connected)
                    {
                        var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage(message.GetMessageSize());

                        message.Serialize(lidgrenMsg);
                        NetworkMain.ClientConnection.SendMessage(lidgrenMsg, message.NetDeliveryMethod, message.Channel);
                    }
                }

                //Force send of packets
                NetworkMain.ClientConnection.FlushSendQueue();
                message.Recycle();
            }
            catch (Exception e)
            {
                NetworkMain.HandleDisconnectException(e);
            }
        }
    }
}
