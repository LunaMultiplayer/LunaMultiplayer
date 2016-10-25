using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaClient.Windows;
using LunaCommon.Enums;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Handshake;
using Lidgren.Network;
using LunaCommon;
using UnityEngine;

namespace LunaClient.Network
{
    public class NetworkConnection
    {
        private static Task ConnectThread { get; set; }
        private static object DisconnectLock { get; } = new object();

        /// <summary>
        /// Disconnects the network system. You should kill threads ONLY from main thread
        /// </summary>
        /// <param name="reason">Reason</param>
        public static void Disconnect(string reason = "unknown")
        {
            lock (DisconnectLock)
            {
                if (MainSystem.Singleton.NetworkState != ClientState.DISCONNECTED)
                {
                    Debug.Log("[LMP]: Disconnected, reason: " + reason);
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
                    NetworkMain.ClientConnection.Disconnect(reason);
                    NetworkMain.ClientConnection.Shutdown(reason);
                    NetworkMain.ResetConnectionStaticsAndQueues();
                    WindowsHandler.Reset();
                }
            }
            NetworkMain.ClientConnection.Start();
        }

        public static void ConnectToServer(string address, int port)
        {
            try
            {
                ConnectThread?.Wait(1000);

                Disconnect("Started a new connection");

                ConnectThread = new Task(() => ConnectToServer(address + ":" + port));
                ConnectThread.Start(TaskScheduler.Default);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Error calling the connect thread {e}");
            }
        }

        #region Private

        public static void ConnectToServer(string endpointString)
        {
            try
            {
                if (MainSystem.Singleton.NetworkState == ClientState.DISCONNECTED)
                {
                    var endpoint = Common.CreateEndpointFromString(endpointString);
                    MainSystem.Singleton.Status = $"Connecting to {endpoint.Address} port {endpoint.Port}";
                    Debug.Log($"[LMP]: Connecting to {endpoint.Address} port {endpoint.Port}");

                    MainSystem.Singleton.NetworkState = ClientState.CONNECTING;
                    ConnectToServerAddress(endpoint);
                }
                else
                {
                    Debug.LogError("[LMP]: Cannot connect when we are already connected!");
                }
            }
            catch (Exception)
            {
                MainSystem.Singleton.Status = "Invalid IP address: " + endpointString;
            }
        }

        private static void ConnectToServerAddress(IPEndPoint destination)
        {
            try
            {
                var outmsg = NetworkMain.ClientConnection.CreateMessage(1);
                outmsg.Write((byte)NetIncomingMessageType.ConnectionApproval);

                NetworkMain.ClientConnection.Start();
                NetworkMain.ClientConnection.Connect(destination);
                NetworkMain.ClientConnection.FlushSendQueue();

                var connectionTrials = 0;
                while ((NetworkMain.ClientConnection.ConnectionStatus == NetConnectionStatus.Disconnected) && (connectionTrials <= SettingsSystem.CurrentSettings.ConnectionTries))
                {
                    connectionTrials++;
                    Thread.Sleep(SettingsSystem.CurrentSettings.MsBetweenConnectionTries);
                }

                if (NetworkMain.ClientConnection.ConnectionStatus != NetConnectionStatus.Disconnected)
                {
                    Debug.Log("[LMP]: Connected to " + destination.Address + " port " + destination.Port);
                    MainSystem.Singleton.Status = "Connected";
                    MainSystem.Singleton.NetworkState = ClientState.CONNECTED;
                    NetworkSender.OutgoingMessages.Enqueue(NetworkMain.CliMsgFactory.CreateNew<HandshakeCliMsg>(new HandshakeRequestMsgData()));
                }
                else
                {
                    Debug.LogError("[LMP]: Failed to connect within the timeout!");
                    Disconnect("Initial connection timeout");
                }
            }
            catch (Exception e)
            {
                NetworkMain.HandleDisconnectException(e);
            }
        }

        #endregion
    }
}