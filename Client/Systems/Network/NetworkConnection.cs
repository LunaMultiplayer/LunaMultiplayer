using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Utilities;
using LunaClient.Windows;
using LunaCommon.Enums;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using Lidgren.Network;
using UnityEngine;

namespace LunaClient.Systems.Network
{
    public partial class NetworkSystem
    {
        public Thread ReceiveThread { get; set; }
        public Thread SendThread { get; set; }

        /// <summary>
        /// Kill all the network threads. Call this method ONLY from the main thread
        /// </summary>
        public void KillThreads()
        {
            ClientConnection?.Shutdown("Disconnected!");

            if (ConnectThread?.IsAlive ?? false)
                ConnectThread.Abort();

            if (ReceiveThread?.IsAlive ?? false)
                ReceiveThread?.Abort();

            if (SendThread?.IsAlive ?? false)
                SendThread?.Abort();

            if (TimeSyncerSystem.Singleton.SyncSenderThread?.IsAlive ?? false)
                TimeSyncerSystem.Singleton.SyncSenderThread?.Abort();
        }

        private static NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {
            AutoFlushSendQueue = false,
            SuppressUnreliableUnorderedAcks = true, //We don't need ack for unreliable unordered!
            MaximumTransmissionUnit = SettingsSystem.CurrentSettings.MtuSize,
            PingInterval = (float)SettingsSystem.CurrentSettings.HearbeatMsInterval / 1000,
            ConnectionTimeout = (float)SettingsSystem.CurrentSettings.ConnectionMsTimeout / 1000
        };

        private object DisconnectLock { get; } = new object();
        public Thread ConnectThread { get; set; }
        
        /// <summary>
        /// Disconnects the network system. You should kill threads ONLY from main thread
        /// </summary>
        /// <param name="reason">Reason</param>
        /// <param name="killThreads">Kill threads ONLY when you call this method from main thread otherwise you'll get locked</param>
        public void Disconnect(string reason = "unknown", bool killThreads = false)
        {
            lock (DisconnectLock)
            {
                if (MainSystem.Singleton.NetworkState != ClientState.DISCONNECTED)
                {
                    LunaLog.Debug("Disconnected, reason: " + reason);
                    if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)
                    {
                        MainSystem.Singleton.ForceQuit = true;
                    }
                    else
                    {
                        //User is in flight so just display a message but don't force him to main menu...
                        MainSystem.Singleton.DisplayDisconnectMessage = true;
                    }

                    MainSystem.Singleton.Status = "Disconnected: " + reason;
                    MainSystem.Singleton.NetworkState = ClientState.DISCONNECTED;
                    ClientConnection?.Shutdown(reason);
                    ResetConnectionStaticsAndQueues();
                    WindowsHandler.Reset();
                }
            }

            if (killThreads)
                KillThreads();
        }
        public IEnumerator UploadScreenshot()
        {
            yield return new WaitForEndOfFrame();
        }

        public void ConnectToServer(string address, int port)
        {
            var connectAddress = new LmpServerAddress
            {
                Ip = address,
                Port = port
            };

            KillThreads();
            ConnectThread = new Thread(ConnectToServerThread) {IsBackground = true};
            ConnectThread.Start(connectAddress);
        }

        #region Private

        private void ConnectToServerThread(object connectAddress)
        {
            var connectAddressCast = (LmpServerAddress)connectAddress;
            var address = connectAddressCast.Ip;
            var port = connectAddressCast.Port;

            if (MainSystem.Singleton.NetworkState == ClientState.DISCONNECTED)
            {
                LunaLog.Debug("Trying to connect to " + address + ", port " + port);
                MainSystem.Singleton.Status = "Connecting to " + address + " port " + port;
                
                IPAddress destinationAddress;
                if (IPAddress.TryParse(address, out destinationAddress))
                {
                    MainSystem.Singleton.Status = "Connecting";
                    MainSystem.Singleton.NetworkState = ClientState.CONNECTING;
                    ConnectToServerAddress(new IPEndPoint(destinationAddress, port));
                }
                else
                {
                    LunaLog.Debug("Invalid IP address: " + address);
                    MainSystem.Singleton.Status = "Invalid IP address: " + address;
                }
            }
        }

        private void ResetConnectionStaticsAndQueues()
        {
            SystemsHandler.ClearMessageQueues();
            OutgoingMessages = new ConcurrentQueue<IClientMessageBase>();
            SettingsSystem.Singleton.ResetServerSettings();
            LastReceiveTime = 0;
            LastSendTime = 0;
        }

        private void ConnectToServerAddress(object destinationObject)
        {
            var destination = (IPEndPoint)destinationObject;

            if (ClientConnection.ConnectionStatus != NetConnectionStatus.Disconnected)
                ClientConnection.Disconnect("Quit");

            ClientConnection.Configuration.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
            ClientConnection.Configuration.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            ClientConnection.Configuration.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            ClientConnection.Start();

            try
            {
                LunaLog.Debug("Connecting to " + destination.Address + " port " + destination.Port + "...");

                var outmsg = ClientConnection.CreateMessage(1);
                outmsg.Write((byte)NetIncomingMessageType.ConnectionApproval);

                ClientConnection.Connect(destination);
                ClientConnection.FlushSendQueue();

                var connectionTrials = 0;
                while ((ClientConnection.ConnectionStatus == NetConnectionStatus.Disconnected) && (connectionTrials <= SettingsSystem.CurrentSettings.ConnectionTries))
                {
                    connectionTrials++;
                    MainSystem.Delay(SettingsSystem.CurrentSettings.MsBetweenConnectionTries);
                }

                if (ClientConnection.ConnectionStatus != NetConnectionStatus.Disconnected)
                {
                    LunaLog.Debug("Connected to " + destination.Address + " port " + destination.Port);
                    MainSystem.Singleton.Status = "Connected";
                    MainSystem.Singleton.NetworkState = ClientState.CONNECTED;

                    ReceiveThread?.Abort();
                    ReceiveThread = new Thread(ReceiveThreadMain) {IsBackground = true};
                    ReceiveThread.Start();

                    SendThread?.Abort();
                    SendThread = new Thread(SendThreadMain) {IsBackground = true};
                    SendThread.Start();

                    OutgoingMessages.Enqueue(MessageFactory.CreateNew<HandshakeCliMsg>(new HandshakeRequestMsgData()));
                }
                else
                {
                    LunaLog.Debug("Failed to connect within the timeout!");
                    Disconnect("Initial connection timeout");
                }
            }
            catch (Exception e)
            {
                HandleDisconnectException(e);
            }
        }

        #endregion
    }
}