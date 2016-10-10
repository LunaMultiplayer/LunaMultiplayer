using System;
using System.Collections.Concurrent;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer;
using UnityEngine;

namespace LunaClient.Network
{
    public class NetworkSender
    {
        public static ConcurrentQueue<IMessageBase> OutgoingMessages { get; set; } = new ConcurrentQueue<IMessageBase>();
        public static NetworkSimpleMessageSender SimpleMessageSender { get; } = new NetworkSimpleMessageSender();

        public static void SendMain()
        {
            try
            {
                while (!MainSystem.Singleton.Quit)
                {
                    //Send master server msgs always
                    IMessageBase sendMessage;
                    if (OutgoingMessages.TryPeek(out sendMessage) && sendMessage is IMasterServerMessageBase)
                    {
                        OutgoingMessages.TryDequeue(out sendMessage);
                        SendNetworkMessage(sendMessage);
                    }
                    else
                    {
                        MainSystem.Delay(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
                    }

                    //ONly send client messages when status is connected
                    while (MainSystem.Singleton.NetworkState >= ClientState.CONNECTED)
                    {
                        if (OutgoingMessages.TryDequeue(out sendMessage))
                        {
                            if (sendMessage is IClientMessageBase)
                                SendNetworkMessage(sendMessage);
                        }
                        else
                        {
                            MainSystem.Delay(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Send thread error: " + e);
            }
        }

        public static void QueueOutgoingMessage(IMessageBase message)
        {
            OutgoingMessages.Enqueue(message);
        }

        private static void SendNetworkMessage(IMessageBase message)
        {
            var clientMessage = message as IClientMessageBase;
            var masterSrvMessage = message as IMasterServerMessageBase;

            if (clientMessage?.MessageType == ClientMessageType.SYNC_TIME)
                TimeSyncerSystem.Singleton.RewriteMessage(message.Data);

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
                        var lidgrenMsg = NetworkMain.ClientConnection.CreateMessage(bytes.Length);
                        lidgrenMsg.Write(message.Serialize(SettingsSystem.CurrentSettings.CompressionEnabled));
                        NetworkMain.ClientConnection.SendMessage(lidgrenMsg, message.NetDeliveryMethod, message.Channel);
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