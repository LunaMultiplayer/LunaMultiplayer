using System;
using System.Threading;
using System.Threading.Tasks;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;
using LunaServer.System;
using Lidgren.Network;

namespace LunaServer.Lidgren
{
    public class LidgrenServer
    {
        private static NetServer Server { get; set; }
        public static MessageReceiver ClientMessageReceiver { get; set; } = new MessageReceiver();

        public void SetupLidgrenServer()
        {
            try
            {
                ServerContext.Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

                Server = new NetServer(ServerContext.Config);
                Server.Start();
            }
            catch (Exception e)
            {
                LunaLog.Normal("Error setting up server, Exception: " + e);
                ServerContext.ServerRunning = false;
            }
            ServerContext.ServerStarting = false;
        }

        public void StartReceiveingMessages()
        {
            try
            {
                while (ServerContext.ServerRunning)
                {
                    var msg = Server.ReadMessage();
                    if (msg != null)
                    {
                        var client = TryGetClient(msg);
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionApproval:
                                msg.SenderConnection.Approve();
                                break;
                            case NetIncomingMessageType.Data:
                                ClientMessageReceiver.ReceiveCallback(client, msg);
                                break;
                            case NetIncomingMessageType.WarningMessage:
                                LunaLog.Error($"Lidgren WARNING: {msg.ReadString()}");
                                break;
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.VerboseDebugMessage:
                                LunaLog.Debug("Lidgren DEBUG: " + msg.MessageType + "-- " + msg.PeekString());
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                switch ((NetConnectionStatus)msg.ReadByte())
                                {
                                    case NetConnectionStatus.Connected:
                                        var endpoint = msg.SenderConnection.RemoteEndPoint;
                                        LunaLog.Normal($"New client Connection from {endpoint.Address}:{endpoint.Port}");
                                        ClientConnectionHandler.ConnectClient(msg.SenderConnection);
                                        break;
                                    case NetConnectionStatus.Disconnected:
                                        var reason = msg.ReadString();
                                        if (client != null)
                                            ClientConnectionHandler.DisconnectClient(client, reason);
                                        break;
                                }
                                break;
                            default:
                                var details = msg.PeekString();
                                LunaLog.Debug("Lidgren: " + msg.MessageType.ToString().ToUpper() + " -- " + details);
                                break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.Fatal($"ERROR in thread receive! Details: {e}");
            }
        }

        private static ClientStructure TryGetClient(NetIncomingMessage msg)
        {
            if (msg.SenderConnection != null)
            {
                ClientStructure client;
                ServerContext.Clients.TryGetValue(msg.SenderConnection.RemoteEndPoint, out client);
                return client;
            }
            return null;
        }

        public void SendMessageToClient(ClientStructure client, IServerMessageBase message)
        {
            if (message.MessageType == ServerMessageType.SYNC_TIME)
                SyncTimeSystem.RewriteMessage(client, message);
            var messageBytes = message.Serialize(GeneralSettings.SettingsStore.CompressionEnabled);

            if (messageBytes == null)
            {
                LunaLog.Error("Error serializing message!");
                return;
            }

            client.LastSendTime = ServerContext.ServerClock.ElapsedMilliseconds;
            client.BytesSent += messageBytes.Length;

            var outmsg = Server.CreateMessage(messageBytes.Length);
            outmsg.Write(messageBytes);

            Server.SendMessage(outmsg, client.Connection, message.NetDeliveryMethod, message.Channel);
            Server.FlushSendQueue(); //Manually force to send the msg
        }

        public void ShutdownLidgrenServer()
        {
            Server.Shutdown("Goodbye and thanks for all the fish");
        }
    }
}