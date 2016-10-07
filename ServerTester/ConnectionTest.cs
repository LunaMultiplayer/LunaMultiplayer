using System;
using System.Net;
using System.Threading;
using Lidgren.Network;
using LunaClient;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message;

namespace ServerTester
{
    class ConnectionTest
    {
        public Thread ReceiveThread { get; set; }
        public Thread SendThread { get; set; }
        private NetClient ClientConnection { get; set; }
        private float Ping { get; set; }
        private static ServerMessageFactory ServerMessageFactory { get; } = new ServerMessageFactory(true);

        private NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {
            AutoFlushSendQueue = false,
            MaximumTransmissionUnit = NetPeerConfiguration.kDefaultMTU,
            SuppressUnreliableUnorderedAcks = true,
#if DEBUG
            PingInterval = 2, //We can implement something like hearbeats but using the library!
            ConnectionTimeout = 5 //Default is 25 seconds
#endif
        };

        private void ConnectToServerAddress(object destinationObject)
        {
            var destination = (IPEndPoint)destinationObject;

            if (ClientConnection != null && ClientConnection.ConnectionStatus != NetConnectionStatus.Disconnected)
                ClientConnection.Disconnect("Goodbye!");

            ClientConnection = new NetClient(Config);
            ClientConnection.Configuration.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            ClientConnection.Configuration.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            ClientConnection.Start();

            try
            {
                LunaLog.Debug("Connecting to " + destination.Address + " port " + destination.Port + "...");

                var outmsg = ClientConnection.CreateMessage(1);
                outmsg.Write((byte)NetIncomingMessageType.ConnectionApproval);

                ClientConnection.Connect(destination);
                ClientConnection.FlushSendQueue();

                var connectionTrials = 0;
                while ((ClientConnection.ConnectionStatus == NetConnectionStatus.Disconnected) && (connectionTrials <= 3))
                {
                    connectionTrials++;
                    MainSystem.Delay(3000);
                }

                if (ClientConnection.ConnectionStatus != NetConnectionStatus.Disconnected)
                {
                    ReceiveThread?.Abort();
                    ReceiveThread = new Thread(ReceiveThreadMain) { IsBackground = true };
                    ReceiveThread.Start();
                }
                else
                {
                    LunaLog.Debug("Failed to connect within the timeout!");
                }
            }
            catch (Exception e)
            {
            }
        }
        
        private void ReceiveThreadMain()
        {
            try
            {
                while (ClientConnection != null && MainSystem.Singleton.NetworkState >= ClientState.CONNECTED)
                {
                    NetIncomingMessage msg;
                    if (ClientConnection.ReadMessage(out msg))
                    {
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                                Ping = msg.ReadFloat() * 1000;
                                break;
                            case NetIncomingMessageType.Data:
                                try
                                {
                                    var deserializedMsg = ServerMessageFactory.Deserialize(msg.ReadBytes(msg.LengthBytes), DateTime.UtcNow.Ticks);
                                    Console.Write("RECEIVED: " + deserializedMsg.GetType() + "-- " + deserializedMsg.Data.GetType());
                                }
                                catch (Exception e)
                                {
                                    LunaLog.Debug("Error deserializing message!");
                                }
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                if (msg.SenderConnection.Status == NetConnectionStatus.Disconnected)
                                    Console.Write("Connection lost with the server");
                                break;
                            default:
                                Console.Write("LIDGREN: " + msg.MessageType + "-- " + msg.PeekString());
                                break;
                        }
                    }
                    else
                    {
                        MainSystem.Delay(20);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
