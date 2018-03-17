using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Bug;
using LunaClient.Systems.Chat;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Facility;
using LunaClient.Systems.Flag;
using LunaClient.Systems.FlagPlant;
using LunaClient.Systems.Groups;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.KscScene;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Motd;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.Screenshot;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselEvaSys;
using LunaClient.Systems.VesselFairingsSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPartModuleSyncSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselPrecalcSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselResourceSys;
using LunaClient.Systems.VesselStateSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.VesselSyncSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;

namespace LunaClient.Systems.Network
{
    public class NetworkSystem : System<NetworkSystem>
    {
        #region Constructor

        /// <summary>
        /// This system must be ALWAYS enabled so we set it as enabled on the constructor
        /// </summary>
        public NetworkSystem()
        {
            base.Enabled = true;
        }

        #endregion

        #region Disconnect message

        public static bool DisplayDisconnectMessage { get; set; }

        #endregion

        public override string SystemName { get; } = nameof(NetworkSystem);

        private static bool _enabled = true;

        /// <summary>
        /// This system must be ALWAYS enabled!
        /// </summary>
        public override bool Enabled
        {
            get => _enabled;
            set
            {
                base.Enabled |= value;
                _enabled |= value;
            }
        }

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, NetworkUpdate));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, ShowDisconnectMessage));
        }

        public override int ExecutionOrder => int.MinValue;

        #endregion

        #region Update method

        private void NetworkUpdate()
        {
            switch (MainSystem.NetworkState)
            {
                case ClientState.DisconnectRequested:
                case ClientState.Disconnected:
                    DisableAllSystems();
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
                    CraftLibrarySystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingCraftlibrary;
                    NetworkSimpleMessageSender.SendCraftLibraryRequest();
                    _lastStateTime = DateTime.Now;
                    break;
                case ClientState.SyncingCraftlibrary:
                    MainSystem.Singleton.Status = "Syncing craft library";
                    if (ConnectionIsStuck(20000))
                        MainSystem.NetworkState = ClientState.ScenariosSynced;
                    break;
                case ClientState.CraftlibrarySynced:
                    MainSystem.Singleton.Status = "Craft library synced";
                    MainSystem.NetworkState = ClientState.SyncingLocks;
                    LockSystem.Singleton.Enabled = true;
                    LockSystem.Singleton.MessageSender.SendLocksRequest();
                    _lastStateTime = DateTime.Now;
                    break;
                case ClientState.SyncingLocks:
                    MainSystem.Singleton.Status = "Syncing locks";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.CraftlibrarySynced;
                    break;
                case ClientState.LocksSynced:
                    MainSystem.Singleton.Status = "Locks synced";
                    AdminSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingAdmins;
                    NetworkSimpleMessageSender.SendAdminsRequest();
                    _lastStateTime = DateTime.Now;
                    break;
                case ClientState.SyncingAdmins:
                    MainSystem.Singleton.Status = "Syncing admins";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.LocksSynced;
                    break;
                case ClientState.AdminsSynced:
                    MainSystem.Singleton.Status = "Admins synced";
                    GroupSystem.Singleton.Enabled = true;
                    MainSystem.NetworkState = ClientState.SyncingGroups;
                    NetworkSimpleMessageSender.SendGroupListRequest();
                    _lastStateTime = DateTime.Now;
                    break;
                case ClientState.SyncingGroups:
                    MainSystem.Singleton.Status = "Syncing groups";
                    if (ConnectionIsStuck())
                        MainSystem.NetworkState = ClientState.AdminsSynced;
                    break;
                case ClientState.GroupsSynced:
                    MainSystem.Singleton.Status = "Groups synced";
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
                        ChatSystem.Singleton.Enabled = true;
                        TimeSyncerSystem.Singleton.Enabled = true;
                        MotdSystem.Singleton.Enabled = true;
                        VesselLockSystem.Singleton.Enabled = true;
                        VesselPositionSystem.Singleton.Enabled = true;
                        VesselFlightStateSystem.Singleton.Enabled = true;
                        VesselRemoveSystem.Singleton.Enabled = true;
                        VesselImmortalSystem.Singleton.Enabled = true;
                        VesselDockSystem.Singleton.Enabled = true;
                        VesselSwitcherSystem.Singleton.Enabled = true;
                        VesselPrecalcSystem.Singleton.Enabled = true;
                        VesselStateSystem.Singleton.Enabled = true;
                        VesselResourceSystem.Singleton.Enabled = true;
                        VesselUpdateSystem.Singleton.Enabled = true;
                        VesselPartModuleSyncSystem.Singleton.Enabled = true;
                        VesselFairingsSystem.Singleton.Enabled = true;
                        KscSceneSystem.Singleton.Enabled = true;
                        AsteroidSystem.Singleton.Enabled = true;
                        FacilitySystem.Singleton.Enabled = true;
                        FlagPlantSystem.Singleton.Enabled = true;
                        VesselEvaSystem.Singleton.Enabled = true;
                        ScreenshotSystem.Singleton.Enabled = true;
                        BugSystem.Singleton.Enabled = true;
                        PlayerColorSystem.Singleton.MessageSender.SendPlayerColorToServer();
                        StatusSystem.Singleton.MessageSender.SendOwnStatus();
                        NetworkSimpleMessageSender.SendMotdRequest();

                        MainSystem.NetworkState = ClientState.Running;
                    }
                    break;
                case ClientState.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (MotdSystem.Singleton.DisplayMotd && HighLogic.LoadedScene != GameScenes.LOADING)
            {
                MotdSystem.Singleton.DisplayMotd = false;
                ScenarioSystem.Singleton.UpgradeTheAstronautComplexSoTheGameDoesntBugOut();
                ScreenMessages.PostScreenMessage(MotdSystem.Singleton.ServerMotd, 10f, ScreenMessageStyle.UPPER_CENTER);
                //Control locks will bug out the space centre sceen, so remove them before starting.
                NetworkMain.DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway();
            }
        }

        private static void DisableAllSystems()
        {
            HandshakeSystem.Singleton.Enabled = false;
            SettingsSystem.Singleton.Enabled = false;
            KerbalSystem.Singleton.Enabled = false;
            VesselProtoSystem.Singleton.Enabled = false;
            VesselSyncSystem.Singleton.Enabled = false;
            WarpSystem.Singleton.Enabled = false;
            PlayerColorSystem.Singleton.Enabled = false;
            FlagSystem.Singleton.Enabled = false;
            StatusSystem.Singleton.Enabled = false;
            PlayerConnectionSystem.Singleton.Enabled = false;
            ScenarioSystem.Singleton.Enabled = false;
            CraftLibrarySystem.Singleton.Enabled = false;
            LockSystem.Singleton.Enabled = false;
            AdminSystem.Singleton.Enabled = false;
            GroupSystem.Singleton.Enabled = false;
            ChatSystem.Singleton.Enabled = false;
            TimeSyncerSystem.Singleton.Enabled = false;
            MotdSystem.Singleton.Enabled = false;
            VesselLockSystem.Singleton.Enabled = false;
            VesselPositionSystem.Singleton.Enabled = false;
            VesselFlightStateSystem.Singleton.Enabled = false;
            VesselRemoveSystem.Singleton.Enabled = false;
            VesselImmortalSystem.Singleton.Enabled = false;
            VesselDockSystem.Singleton.Enabled = false;
            VesselSwitcherSystem.Singleton.Enabled = false;
            VesselPrecalcSystem.Singleton.Enabled = false;
            VesselStateSystem.Singleton.Enabled = false;
            VesselResourceSystem.Singleton.Enabled = false;
            VesselUpdateSystem.Singleton.Enabled = false;
            VesselPartModuleSyncSystem.Singleton.Enabled = false;
            VesselFairingsSystem.Singleton.Enabled = false;
            KscSceneSystem.Singleton.Enabled = false;
            AsteroidSystem.Singleton.Enabled = false;
            FacilitySystem.Singleton.Enabled = false;
            FlagPlantSystem.Singleton.Enabled = false;
            VesselEvaSystem.Singleton.Enabled = false;
            ScreenshotSystem.Singleton.Enabled = false;
            BugSystem.Singleton.Enabled = false;
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