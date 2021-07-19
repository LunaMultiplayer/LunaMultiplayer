using Lidgren.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon;
using LmpCommon.Enums;
using LmpCommon.Message.Interface;
using LmpCommon.RepoRetrievers;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace LmpClient.Network
{
    public class NetworkSender
    {
        public static ConcurrentQueue<IMessageBase> OutgoingMessages { get; set; } = new ConcurrentQueue<IMessageBase>();

        /// <summary>
        /// Main sending thread
        /// </summary>
        public static void SendMain()
        {
            LunaLog.Log("[LMP]: Send thread started");
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
            LunaLog.Log("[LMP]: Send thread exited");
        }

        /// <summary>
        /// Adds a new message to the queue
        /// </summary>
        public static void QueueOutgoingMessage(IMessageBase message)
        {
            OutgoingMessages.Enqueue(message);
        }

        /// <summary>
        /// Sends the network message. It will skip client messages to send when we are not connected,
        /// except if it's directed at master servers, then it will start the NetClient and socket.
        /// </summary>
        private static void SendNetworkMessage(IMessageBase message)
        {
            message.Data.SentTime = LunaNetworkTime.UtcNow.Ticks;
            try
            {
                if (message is IMasterServerMessageBase)
                {
                    if (NetworkMain.ClientConnection.Status == NetPeerStatus.NotRunning)
                    {
                        LunaLog.Log("Starting client to send unconnected message");
                        NetworkMain.ClientConnection.Start();
                    }
                    while (NetworkMain.ClientConnection.Status != NetPeerStatus.Running)
                    {
                        LunaLog.Log("Waiting for client to start up to send unconnected message");
                        // Still trying to start up
                        Thread.Sleep(50);
                    }

                    IPEndPoint[] masterServers;
                    if (string.IsNullOrEmpty(SettingsSystem.CurrentSettings.CustomMasterServer))
                        masterServers = MasterServerRetriever.MasterServers.GetValues;
                    else
                    {
                        masterServers = new[]
                        {
                            LunaNetUtils.CreateEndpointFromString(SettingsSystem.CurrentSettings.CustomMasterServer)
                        };

                    }
                    foreach (var masterServer in masterServers)
                    {
                        // Don't reuse lidgren messages, he does that on it's own
                        var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage(message.GetMessageSize());

                        message.Serialize(lidgrenMsg);
                        NetworkMain.ClientConnection.SendUnconnectedMessage(lidgrenMsg, masterServer);
                    }
                    // Force send of packets
                    NetworkMain.ClientConnection.FlushSendQueue();
                }
                else
                {
                    if (NetworkMain.ClientConnection == null || NetworkMain.ClientConnection.Status == NetPeerStatus.NotRunning
                        || MainSystem.NetworkState < ClientState.Connected)
                    {
                        return;
                    }
                    var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage(message.GetMessageSize());

                    message.Serialize(lidgrenMsg);
                    NetworkMain.ClientConnection.SendMessage(lidgrenMsg, message.NetDeliveryMethod, message.Channel);
                    // Force send of packets
                    NetworkMain.ClientConnection.FlushSendQueue();
                }

                message.Recycle();
            }
            catch (Exception e)
            {
                NetworkMain.HandleDisconnectException(e);
            }
        }
    }
}
