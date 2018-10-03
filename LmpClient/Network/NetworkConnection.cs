using Lidgren.Network;
using LmpClient.Base;
using LmpClient.Systems.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon;
using LmpCommon.Enums;
using LmpCommon.Message.Base;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LmpClient.Network
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
                        MainSystem.Singleton.ForceQuit = true;
                    }
                    else
                    {
                        //User is in flight so just display a message but don't force him to main menu...
                        NetworkSystem.DisplayDisconnectMessage = true;
                    }

                    MainSystem.Singleton.Status = $"Disconnected: {reason}";
                    
                    NetworkMain.ClientConnection.Disconnect(reason);
                    NetworkMain.ClientConnection.Shutdown(reason);
                    NetworkMain.ResetConnectionStaticsAndQueues();
                }
            }
        }

        public static void ConnectToServer(string address, int port, string password)
        {
            try
            {
                while (!ConnectThread?.IsCompleted ?? false)
                {
                    Thread.Sleep(500);
                }

                ConnectThread = SystemBase.TaskFactory.StartNew(() => ConnectToServer($"{address}:{port}", password));
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error calling the connect thread {e}");
            }
        }

        #region Private

        private static void ConnectToServer(string endpointString, string serverPassword)
        {
            if (NetworkMain.ClientConnection.Status == NetPeerStatus.NotRunning)
                NetworkMain.ClientConnection.Start();

            if (MainSystem.NetworkState <= ClientState.Disconnected)
            {
                var endpoint = CreateEndpoint(endpointString);
                if (endpoint == null) return;

                MainSystem.Singleton.Status = $"Connecting to {endpoint.Address}:{endpoint.Port}";
                LunaLog.Log($"[LMP]: Connecting to {endpoint.Address} port {endpoint.Port}");

                MainSystem.NetworkState = ClientState.Connecting;
                try
                {
                    var outmsg = NetworkMain.ClientConnection.CreateMessage(serverPassword.GetByteCount());
                    outmsg.Write(serverPassword);
                    
                    NetworkMain.ClientConnection.Connect(endpoint, outmsg);

                    //Force send of packets
                    NetworkMain.ClientConnection.FlushSendQueue();

                    var connectionTrials = 0;
                    while (MainSystem.NetworkState >= ClientState.Connecting &&
                            NetworkMain.ClientConnection.ConnectionStatus != NetConnectionStatus.Connected &&
                           connectionTrials < SettingsSystem.CurrentSettings.ConnectionTries)
                    {
                        connectionTrials++;
                        Thread.Sleep(SettingsSystem.CurrentSettings.MsBetweenConnectionTries);
                    }

                    if (NetworkMain.ClientConnection.ConnectionStatus == NetConnectionStatus.Connected)
                    {
                        LunaLog.Log($"[LMP]: Connected to {endpoint.Address}:{endpoint.Port}");
                        MainSystem.NetworkState = ClientState.Connected;
                    }
                    else
                    {
                        Disconnect(MainSystem.NetworkState == ClientState.Connecting ? "Initial connection timeout" : "Cancelled connection");
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
                MainSystem.Singleton.Status = $"Invalid IP address: {endpointString}";
                return null;
            }
        }
        
        #endregion
    }
}