using Lidgren.Network;
using LunaClient.Base;
using LunaClient.Systems;
using LunaClient.Systems.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon;
using LunaCommon.Enums;
using System;
using System.Net;
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
                while (!ConnectThread?.IsCompleted ?? false)
                {
                    LunaDelay.Delay(500);
                }

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
            if (MainSystem.NetworkState <= ClientState.Disconnected)
            {
                var endpoint = CreateEndpoint(endpointString);
                if (endpoint == null) return;

                SystemsContainer.Get<MainSystem>().Status = $"Connecting to {endpoint.Address}:{endpoint.Port}";
                LunaLog.Log($"[LMP]: Connecting to {endpoint.Address} port {endpoint.Port}");

                MainSystem.NetworkState = ClientState.Connecting;
                try
                {
                    var outmsg = NetworkMain.ClientConnection.CreateMessage(1);
                    outmsg.Write((byte)NetIncomingMessageType.ConnectionApproval);

                    NetworkMain.ClientConnection.Start();
                    NetworkMain.ClientConnection.Connect(endpoint);
                    NetworkMain.ClientConnection.FlushSendQueue();

                    var connectionTrials = 0;
                    while (MainSystem.NetworkState >= ClientState.Connecting &&
                            NetworkMain.ClientConnection.ConnectionStatus != NetConnectionStatus.Connected &&
                           connectionTrials < SettingsSystem.CurrentSettings.ConnectionTries)
                    {
                        connectionTrials++;
                        LunaDelay.Delay(SettingsSystem.CurrentSettings.MsBetweenConnectionTries);
                    }

                    if (NetworkMain.ClientConnection.ConnectionStatus == NetConnectionStatus.Connected)
                    {
                        LunaLog.Log($"[LMP]: Connected to {endpoint.Address}:{endpoint.Port}");
                        MainSystem.NetworkState = ClientState.Connected;
                    }
                    else
                    {
                        if (MainSystem.NetworkState == ClientState.Connecting)
                            Disconnect("Initial connection timeout");
                        else
                            Disconnect("Cancelled connection");
                    }
                }
                catch (Exception e)
                {
                    NetworkMain.HandleDisconnectException(e);
                }
            }
            else
            {
                LunaLog.LogError("[LMP]: Cannot connect when we are already trying to connect");
            }
        }

        private static IPEndPoint CreateEndpoint(string endpointString)
        {
            try
            {
                return Common.CreateEndpointFromString(endpointString);
            }
            catch (Exception)
            {
                SystemsContainer.Get<MainSystem>().Status = $"Invalid IP address: {endpointString}";
                return null;
            }
        }
        
        #endregion
    }
}