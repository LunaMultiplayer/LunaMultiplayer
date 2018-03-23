using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Systems.Flag;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselSyncSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;

namespace LunaClient.Systems.Network
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
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, ShowDisconnectMessage));
        }

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(NetworkSystem);

        protected override bool AlwaysEnabled => true;
        
        public override int ExecutionOrder => int.MinValue;

        #endregion

        #region Update method

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
                    NetworkSimpleMessageSender.SendHandshakeRequest();
                    _lastStateTime = DateTime.Now;
                    break;
                case ClientState.Handshaking:
                    MainSystem.Singleton.Status = "Waiting for handshake challenge";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.Connected;
                    break;
                case ClientState.HandshakeChallengeReceived:
                    MainSystem.Singleton.Status = "Challenge received, authenticating";
                    MainSystem.NetworkState = ClientState.Authenticating;
                    HandshakeSystem.Singleton.SendHandshakeChallengeResponse();
                    _lastStateTime = DateTime.Now;
                    break;
                case ClientState.Authenticating:
                    MainSystem.Singleton.Status = "Connection successful, authenticating";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.HandshakeChallengeReceived;
                    break;
                case ClientState.Authenticated:
                    MainSystem.Singleton.Status = "Handshaking successful";
                    SettingsSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingSettings;
                    NetworkSimpleMessageSender.SendSettingsRequest();

                    _lastStateTime = DateTime.Now;
                    break;
                case ClientState.SyncingSettings:
                    MainSystem.Singleton.Status = "Syncing settings";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.Authenticated;
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
                        NetworkSimpleMessageSender.SendKerbalsRequest();
                        _lastStateTime = DateTime.Now;
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
                    NetworkSimpleMessageSender.SendWarpSubspacesRequest();
                    _lastStateTime = DateTime.Now;
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
                    NetworkSimpleMessageSender.SendColorsRequest();
                    _lastStateTime = DateTime.Now;
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
                    NetworkSimpleMessageSender.SendFlagsRequest();
                    _lastStateTime = DateTime.Now;
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
                    NetworkSimpleMessageSender.SendPlayersRequest();
                    _lastStateTime = DateTime.Now;
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
                    NetworkSimpleMessageSender.SendScenariosRequest();
                    _lastStateTime = DateTime.Now;
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
                    _lastStateTime = DateTime.Now;
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
            if ((DateTime.Now - _lastStateTime).TotalMilliseconds > maxIdleMiliseconds)
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
                ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.Disconected, 2f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        #endregion
    }
}
