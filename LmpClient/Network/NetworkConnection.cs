using Lidgren.Network;
using LmpClient.Base;
using LmpClient.ModuleStore.Patching;
using LmpClient.Systems.Network;
using LmpCommon;
using LmpCommon.Enums;
using LmpCommon.Message.Base;
using System;
using System.Net;
using System.Threading;
using UniLinq;

namespace LmpClient.Network
{
    public class NetworkConnection
    {
        private static readonly object DisconnectLock = new object();
        public static volatile bool ResetRequested;

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

        public static void ConnectToServer(string hostname, int port, string password)
        {
            ConnectToServer(
                LunaNetUtils.CreateAddressFromString(hostname).Select(ep => new IPEndPoint(ep, port)).ToArray(),
                password
            );
        }

        public static void ConnectToServer(IPEndPoint[] endpoints, string password)
        {
            if (MainSystem.NetworkState > ClientState.Disconnected || endpoints == null || endpoints.Length == 0)
                return;

            MainSystem.NetworkState = ClientState.Connecting;

            SystemBase.TaskFactory.StartNew(() =>
            {
                while (!PartModuleRunner.Ready)
                {
                    MainSystem.Singleton.Status = $"Patching part modules (runs on every restart). {PartModuleRunner.GetPercentage()}%";
                    Thread.Sleep(50);
                }

                foreach (var endpoint in endpoints)
                {
                    if (endpoint == null)
                        continue;
                    MainSystem.Singleton.Status = $"Connecting to {endpoint.Address}:{endpoint.Port}";
                    LunaLog.Log($"[LMP]: Connecting to {endpoint.Address} port {endpoint.Port}");

                    try
                    {
                        var client = NetworkMain.ClientConnection;

                        if (client.Status == NetPeerStatus.NotRunning)
                        {
                            LunaLog.Log("[LMP]: Starting client");
                            client.Start();
                        }

                        while (client.Status != NetPeerStatus.Running)
                        {
                            // Still trying to start up
                            Thread.Sleep(50);
                        }

                        var outMsg = client.CreateMessage(password.GetByteCount());
                        outMsg.Write(password);

                        var conn = client.Connect(endpoint, outMsg);
                        if (conn == null)
                        {
                            // Lidgren says we're already connected, that's not possible
                            LunaLog.LogError($"[LMP]: Invalid connection state, connected without connection");
                            client.Disconnect("Invalid state");
                            break;
                        }
                        client.FlushSendQueue();

                        while (conn.Status == NetConnectionStatus.InitiatedConnect || conn.Status == NetConnectionStatus.None)
                        {
                            // Still trying to connect
                            Thread.Sleep(50);
                        }

                        if (client.ConnectionStatus == NetConnectionStatus.Connected)
                        {
                            LunaLog.Log($"[LMP]: Connected to {endpoint.Address}:{endpoint.Port}");
                            MainSystem.NetworkState = ClientState.Connected;
                            break;
                        }
                        else
                        {
                            LunaLog.Log($"[LMP]: Initial connection timeout to {endpoint.Address}:{endpoint.Port}");
                            client.Disconnect("Initial connection timeout");
                        }
                    }
                    catch (Exception e)
                    {
                        NetworkMain.HandleDisconnectException(e);
                    }
                }

                if (MainSystem.NetworkState < ClientState.Connected)
                {
                    Disconnect(MainSystem.NetworkState == ClientState.Connecting ? "Initial connection timeout" : "Cancelled connection");
                }
            });
        }
    }
}