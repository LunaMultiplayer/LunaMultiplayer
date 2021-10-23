using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Localization;
using LmpClient.Network;
using LmpClient.Systems.Flag;
using LmpClient.Systems.Handshake;
using LmpClient.Systems.KerbalSys;
using LmpClient.Systems.Lock;
using LmpClient.Systems.PlayerColorSys;
using LmpClient.Systems.PlayerConnection;
using LmpClient.Systems.Scenario;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.Status;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselSyncSys;
using LmpClient.Systems.Warp;
using LmpClient.Utilities;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;

namespace LmpClient.Systems.Network
{
    public class NetworkSystem : System<NetworkSystem>
    {
        #region Disconnect message

        public static bool DisplayDisconnectMessage { get; set; }

        #endregion

        #region Constructor

        public NetworkSystem()
        {
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, NetworkUpdate));
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, ProcessNetworkStatusChanges));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, ShowDisconnectMessage));
        }

        #endregion

        #region Fields and properties

        public static ClientState? NetworkStatus { private get; set; }

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(NetworkSystem);

        protected override bool AlwaysEnabled => true;

        public override int ExecutionOrder => int.MinValue;

        #endregion

        #region Routines

        /// <summary>
        /// This routine checks if the network status has changed and triggers the event in the UNITY thread
        /// </summary>
        private static void ProcessNetworkStatusChanges()
        {
            if (NetworkStatus.HasValue)
            {
                NetworkEvent.onNetworkStatusChanged.Fire(NetworkStatus.Value);
                NetworkStatus = null;
            }
        }

        private void NetworkUpdate()
        {
            switch (MainSystem.NetworkState)
            {
                case ClientState.DisconnectRequested:
                case ClientState.Disconnected:
                    break;
                case ClientState.Connecting:
                    ChangeRoutineExecutionInterval(RoutineExecution.Update, nameof(NetworkUpdate), 0);
                    return;
                case ClientState.Connected:
                    HandshakeSystem.Singleton.Enabled = true;
                    MainSystem.Singleton.Status = "Connected";
                    MainSystem.NetworkState = ClientState.Handshaking;
                    HandshakeSystem.Singleton.MessageSender.SendHandshakeRequest();
                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.Handshaking:
                    MainSystem.Singleton.Status = "Waiting for handshake response";
                    if (ConnectionIsStuck(60000))
                    {
                        LunaLog.Log("[LMP]: Failed to get a handshake response after 60 seconds. Sending the handshake again...");
                        MainSystem.NetworkState = ClientState.Connected;
                    }
                    break;
                case ClientState.Handshaked:
                    MainSystem.Singleton.Status = "Handshaking successful";
                    SettingsSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingSettings;
                    SettingsSystem.Singleton.MessageSender.SendSettingsRequest();

                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.SyncingSettings:
                    MainSystem.Singleton.Status = "Syncing settings";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.Handshaked;
                    break;
                case ClientState.SettingsSynced:
                    MainSystem.Singleton.Status = "Settings synced";
                    if (SettingsSystem.ValidateSettings())
                    {
                        KerbalSystem.Singleton.Enabled = true;
                        VesselProtoSystem.Singleton.Enabled = true;
                        VesselSyncSystem.Singleton.Enabled = true;
                        VesselSyncSystem.Singleton.MessageSender.SendVesselsSyncMsg();
                        MainSystem.NetworkState = ClientState.SyncingKerbals;
                        KerbalSystem.Singleton.MessageSender.SendKerbalsRequest();
                        _lastStateTime = LunaComputerTime.UtcNow;
                    }
                    break;
                case ClientState.SyncingKerbals:
                    MainSystem.Singleton.Status = "Syncing kerbals";
                    if (ConnectionIsStuck(10000))
                        MainSystem.NetworkState = ClientState.SettingsSynced;
                    break;
                case ClientState.KerbalsSynced:
                    MainSystem.Singleton.Status = "Kerbals synced";
                    WarpSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingWarpsubspaces;
                    WarpSystem.Singleton.MessageSender.SendWarpSubspacesRequest();
                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.SyncingWarpsubspaces:
                    MainSystem.Singleton.Status = "Syncing warp subspaces";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.KerbalsSynced;
                    break;
                case ClientState.WarpsubspacesSynced:
                    MainSystem.Singleton.Status = "Warp subspaces synced";
                    PlayerColorSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingColors;
                    PlayerColorSystem.Singleton.MessageSender.SendColorsRequest();
                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.SyncingColors:
                    MainSystem.Singleton.Status = "Syncing player colors";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.WarpsubspacesSynced;
                    break;
                case ClientState.ColorsSynced:
                    MainSystem.Singleton.Status = "Player colors synced";
                    FlagSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingFlags;
                    FlagSystem.Singleton.MessageSender.SendFlagsRequest();
                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.SyncingFlags:
                    MainSystem.Singleton.Status = "Syncing flags";
                    if (ConnectionIsStuck(10000))
                        MainSystem.NetworkState = ClientState.ColorsSynced;
                    break;
                case ClientState.FlagsSynced:
                    MainSystem.Singleton.Status = "Flags synced";
                    StatusSystem.Singleton.Enabled = true;
                    PlayerConnectionSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingPlayers;
                    StatusSystem.Singleton.MessageSender.SendPlayersRequest();
                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.SyncingPlayers:
                    MainSystem.Singleton.Status = "Syncing players";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.FlagsSynced;
                    break;
                case ClientState.PlayersSynced:
                    MainSystem.Singleton.Status = "Players synced";
                    ScenarioSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingScenarios;
                    ScenarioSystem.Singleton.MessageSender.SendScenariosRequest();
                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.SyncingScenarios:
                    MainSystem.Singleton.Status = "Syncing scenarios";
                    if (ConnectionIsStuck(10000))
                        MainSystem.NetworkState = ClientState.PlayersSynced;
                    break;
                case ClientState.ScenariosSynced:
                    MainSystem.Singleton.Status = "Scenarios synced";
                    MainSystem.NetworkState = ClientState.SyncingLocks;
                    LockSystem.Singleton.Enabled = true;
                    LockSystem.Singleton.MessageSender.SendLocksRequest();
                    _lastStateTime = LunaComputerTime.UtcNow;
                    break;
                case ClientState.SyncingLocks:
                    MainSystem.Singleton.Status = "Syncing locks";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.ScenariosSynced;
                    break;
                case ClientState.LocksSynced:
                    MainSystem.Singleton.Status = "Starting";
                    MainSystem.Singleton.StartGame = true;
                    MainSystem.NetworkState = ClientState.Starting;
                    break;
                case ClientState.Starting:
                    //Once we start the game we don't need to run this routine on every frame
                    ChangeRoutineExecutionInterval(RoutineExecution.Update, nameof(NetworkUpdate), 1500);
                    MainSystem.Singleton.Status = "Running";
                    CommonUtil.Reserve20Mb();
                    LunaLog.Log("[LMP]: All systems up and running. Поехали!");
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        MainSystem.NetworkState = ClientState.Running;
                        NetworkMain.DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway();
                    }
                    break;
                case ClientState.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Private methods

        private static DateTime _lastStateTime = DateTime.MinValue;
        private static bool ConnectionIsStuck(int maxIdleMiliseconds = 2000)
        {
            if ((LunaComputerTime.UtcNow - _lastStateTime).TotalMilliseconds > maxIdleMiliseconds)
            {
                LunaLog.LogWarning($"Connection got stuck while connecting after waiting {maxIdleMiliseconds} ms, resending last request!");
                return true;
            }

            return false;
        }

        private static void ShowDisconnectMessage()
        {
            if (HighLogic.LoadedScene < GameScenes.SPACECENTER)
                DisplayDisconnectMessage = false;

            if (DisplayDisconnectMessage)
            {
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.Disconected, 2f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        #endregion
    }
}
