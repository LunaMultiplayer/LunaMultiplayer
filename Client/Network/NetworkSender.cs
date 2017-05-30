using Lidgren.Network;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Threading;
using LunaClient.Systems;
using UnityEngine;

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
                while (!SystemsContainer.Get<MainSystem>().Quit)
                {
                    if (OutgoingMessages.TryDequeue(out var sendMessage))
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

            var clientMessage = message as IClientMessageBase;
            var masterSrvMessage = message as IMasterServerMessageBase;

            if (clientMessage?.MessageType == ClientMessageType.SyncTime)
                SystemsContainer.Get<TimeSyncerSystem>().RewriteMessage(message.Data);

            message.Data.SentTime = DateTime.UtcNow.Ticks;
            var bytes = message.Serialize(SettingsSystem.CurrentSettings.CompressionEnabled);
            if (bytes != null)
            {
                try
                {
                    NetworkStatistics.LastSendTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

                    if (masterSrvMessage != null && clientMessage == null)
                    {
                        foreach (var masterServer in NetworkServerList.MasterServers)
                        {
                            //Create a new message for every main server otherwise lidgren complains when you reuse the msg
                            var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage(bytes.Length);
                            lidgrenMsg.Write(message.Serialize(SettingsSystem.CurrentSettings.CompressionEnabled));

                            NetworkMain.ClientConnection.SendUnconnectedMessage(lidgrenMsg, masterServer);
                            NetworkMain.ClientConnection.FlushSendQueue();
                        }
                    }
                    else
                    {
                        if (SystemsContainer.Get<MainSystem>().NetworkState >= ClientState.Connected)
                        {
                            var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage(bytes.Length);
                            lidgrenMsg.Write(message.Serialize(SettingsSystem.CurrentSettings.CompressionEnabled));
                            NetworkMain.ClientConnection.SendMessage(lidgrenMsg, message.NetDeliveryMethod, message.Channel);
                        }
                    }

                    NetworkMain.ClientConnection.FlushSendQueue();
                }
                catch (Exception e)
                {
                    NetworkMain.HandleDisconnectException(e);
                }
            }
        }
    }
}