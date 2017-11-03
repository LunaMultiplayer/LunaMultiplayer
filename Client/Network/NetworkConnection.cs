using Lidgren.Network;
using LunaClient.Base;
using LunaClient.Systems;
using LunaClient.Systems.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Handshake;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LunaClient.Network
{
    public class NetworkConnection
    {
        public static bool ResetRequested { get; set; }
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
                if (MainSystem.NetworkState > ClientState.Disconnected)
                {                    
                    //DO NOT set networkstate as disconnected as we are in another thread!
                    MainSystem.NetworkState = ClientState.DisconnectRequested;

                    LunaLog.Log($"[LMP]: Disconnected, reason: {reason}");
                    if (!HighLogic.LoadedSceneIsEditor && !HighLogic.LoadedSceneIsFlight)
                    {
                        SystemsContainer.Get<MainSystem>().ForceQuit = true;
                    }
                    else
                    {
                        //User is in flight so just display a message but don't force him to main menu...
                        NetworkSystem.DisplayDisconnectMessage = true;
                    }

                    SystemsContainer.Get<MainSystem>().Status = $"Disconnected: {reason}";
                    
                    NetworkMain.ClientConnection.Disconnect(reason);
                    NetworkMain.ClientConnection.Shutdown(reason);
                    NetworkMain.ResetConnectionStaticsAndQueues();
                }
            }
        }

        public static void ConnectToServer(string address, int port)
        {
            try
            {
                ConnectThread?.Wait(1000);

                Disconnect("Started a new connection");

                ConnectThread = SystemBase.TaskFactory.StartNew(() => ConnectToServer($"{address}:{port}"));
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error calling the connect thread {e}");
            }
        }

        #region Private

        public static void ConnectToServer(string endpointString)
        {
            try
            {
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    var endpoint = Common.CreateEndpointFromString(endpointString);
                    SystemsContainer.Get<MainSystem>().Status = $"Connecting to {endpoint.Address} port {endpoint.Port}";
                    LunaLog.Log($"[LMP]: Connecting to {endpoint.Address} port {endpoint.Port}");

                    MainSystem.NetworkState = ClientState.Connecting;
                    ConnectToServerAddress(endpoint);
                }
                else
                {
                    LunaLog.LogError("[LMP]: Cannot connect when we are already connected!");
                }
            }
            catch (Exception)
            {
                SystemsContainer.Get<MainSystem>().Status = $"Invalid IP address: {endpointString}";
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
                while (MainSystem.NetworkState == ClientState.Connecting &&
                    NetworkMain.ClientConnection.ConnectionStatus == NetConnectionStatus.Disconnected &&
                    connectionTrials <= SettingsSystem.CurrentSettings.ConnectionTries)
                {
                    connectionTrials++;
                    Thread.Sleep(SettingsSystem.CurrentSettings.MsBetweenConnectionTries);
                }

                if (NetworkMain.ClientConnection.ConnectionStatus != NetConnectionStatus.Disconnected)
                {
                    LunaLog.Log($"[LMP]: Connected to {destination.Address} port {destination.Port}");
                    SystemsContainer.Get<MainSystem>().Status = "Connected";
                    MainSystem.NetworkState = ClientState.Connected;
                    NetworkSender.OutgoingMessages.Enqueue(NetworkMain.CliMsgFactory.CreateNew<HandshakeCliMsg>(new HandshakeRequestMsgData()));
                }
                else if (MainSystem.NetworkState == ClientState.Connecting)
                {
                    LunaLog.LogError("[LMP]: Failed to connect within the timeout!");
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