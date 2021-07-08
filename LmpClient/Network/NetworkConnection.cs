using Lidgren.Network;
using LmpClient.Base;
using LmpClient.ModuleStore.Patching;
using LmpClient.Systems.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon;
using LmpCommon.Enums;
using LmpCommon.Message.Base;
using System;
using System.Net;
using System.Threading;

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

        public static void ConnectToServer(string address, int port, string password)
        {
            ConnectToServer(new IPEndPoint(LunaNetUtils.CreateAddressFromString(address), port), password);
        }

        public static void ConnectToServer(IPEndPoint endpoint, string password)
        {
            if (MainSystem.NetworkState > ClientState.Disconnected || endpoint == null)
                return;

            MainSystem.NetworkState = ClientState.Connecting;

            if (NetworkMain.ClientConnection.Status == NetPeerStatus.NotRunning)
                NetworkMain.ClientConnection.Start();

            SystemBase.TaskFactory.StartNew(() =>
            {
                while (!PartModuleRunner.Ready)
                {
                    MainSystem.Singleton.Status = $"Patching part modules (runs on every restart). {PartModuleRunner.GetPercentage()}%";
                    Thread.Sleep(50);
                }

                MainSystem.Singleton.Status = $"Connecting to {endpoint.Address}:{endpoint.Port}";
                LunaLog.Log($"[LMP]: Connecting to {endpoint.Address} port {endpoint.Port}");

                try
                {
                    var outMsg = NetworkMain.ClientConnection.CreateMessage(password.GetByteCount());
                    outMsg.Write(password);

                    NetworkMain.ClientConnection.Connect(endpoint, outMsg);
                    NetworkMain.ClientConnection.FlushSendQueue();
                    Thread.Sleep(SettingsSystem.CurrentSettings.MsBetweenConnectionTries);

                    var connectionTrials = 0;
                    while (NetworkMain.ClientConnection.ConnectionStatus != NetConnectionStatus.Connected &&
                           connectionTrials < SettingsSystem.CurrentSettings.ConnectionTries &&
                           MainSystem.NetworkState == ClientState.Connecting)
                    {
                        connectionTrials++;

                        MainSystem.Singleton.Status = $"Connection retry [{connectionTrials + 1}] ({endpoint.Address}:{endpoint.Port})";
                        LunaLog.Log($"[LMP]: Connection retry [{connectionTrials}] ({endpoint.Address}:{endpoint.Port})");

                        NetworkMain.ClientConnection.Connect(endpoint, outMsg);
                        NetworkMain.ClientConnection.FlushSendQueue();
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
            });
        }
    }
}