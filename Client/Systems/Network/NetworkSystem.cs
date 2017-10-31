using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Flag;
using LunaClient.Systems.GameScene;
using LunaClient.Systems.Groups;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalReassigner;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Motd;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.VesselChangeSys;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPositionAltSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
using LunaCommon.Enums;
using System;

namespace LunaClient.Systems.Network
{
    public class NetworkSystem : Base.System
    {
        #region Disconnect message

        public static bool DisplayDisconnectMessage { get; set; }

        #endregion

        /// <summary>
        /// This system must be ALWAYS enabled!
        /// </summary>
        public override bool Enabled => true;

        #region Constructor

        public NetworkSystem()
        {
            //We setup the routine in the constructor as this system is always enabled
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, NetworkUpdate));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, ShowDisconnectMessage));
        }

        #endregion

        #region Update method

        private static void NetworkUpdate()
        {
            switch (MainSystem.NetworkState)
            {
                case ClientState.DisconnectRequested:
                case ClientState.Disconnected:
                case ClientState.Connecting:
                    return;
                case ClientState.Connected:
                    SystemsContainer.Get<HandshakeSystem>().Enabled = true;
                    break;
                case ClientState.Handshaking:
                    SystemsContainer.Get<MainSystem>().Status = "Connection successful, handshaking";
                    break;
                case ClientState.Authenticated:
                    SystemsContainer.Get<PlayerConnectionSystem>().Enabled = true;
                    SystemsContainer.Get<StatusSystem>().Enabled = true;
                    SystemsContainer.Get<StatusSystem>().MessageSender.SendPlayerStatus(SystemsContainer.Get<StatusSystem>().MyPlayerStatus);
                    MainSystem.NetworkState = ClientState.TimeSyncing;
                    break;
                case ClientState.TimeSyncing:
                    SystemsContainer.Get<MainSystem>().Status = "Handshaking successful, syncing server clock";
                    SystemsContainer.Get<TimeSyncerSystem>().Enabled = true;
                    if (SystemsContainer.Get<TimeSyncerSystem>().Synced)
                        MainSystem.NetworkState = ClientState.TimeSynced;
                    else
                        SystemsContainer.Get<TimeSyncerSystem>().MessageSender.SendTimeSyncRequest();
                    break;
                case ClientState.TimeSynced:
                    LunaLog.Log("[LMP]: Time Synced!");
                    SystemsContainer.Get<KerbalSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendKerbalsRequest();
                    MainSystem.NetworkState = ClientState.SyncingKerbals;
                    break;
                case ClientState.SyncingKerbals:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing kerbals";
                    break;
                case ClientState.KerbalsSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Kerbals synced";
                    SystemsContainer.Get<SettingsSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendSettingsRequest();
                    MainSystem.NetworkState = ClientState.SyncingSettings;
                    break;
                case ClientState.SyncingSettings:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing settings";
                    break;
                case ClientState.SettingsSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Settings synced";
                    SystemsContainer.Get<WarpSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendWarpSubspacesRequest();
                    MainSystem.NetworkState = ClientState.SyncingWarpsubspaces;
                    break;
                case ClientState.SyncingWarpsubspaces:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing warp subspaces";
                    break;
                case ClientState.WarpsubspacesSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Warp subspaces synced";
                    SystemsContainer.Get<PlayerColorSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendColorsRequest();
                    MainSystem.NetworkState = ClientState.SyncingColors;
                    break;
                case ClientState.SyncingColors:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing player colors";
                    break;
                case ClientState.ColorsSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Player colors synced";
                    NetworkSimpleMessageSender.SendPlayersRequest();
                    MainSystem.NetworkState = ClientState.SyncingPlayers;
                    break;
                case ClientState.SyncingPlayers:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing players";
                    break;
                case ClientState.PlayersSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Players synced";
                    SystemsContainer.Get<ScenarioSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendScenariosRequest();
                    MainSystem.NetworkState = ClientState.SyncingScenarios;
                    break;
                case ClientState.SyncingScenarios:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing scenarios";
                    break;
                case ClientState.ScneariosSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Scenarios synced";
                    SystemsContainer.Get<CraftLibrarySystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendCraftLibraryRequest();
                    MainSystem.NetworkState = ClientState.SyncingCraftlibrary;
                    break;
                case ClientState.SyncingCraftlibrary:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing craft library";
                    break;
                case ClientState.CraftlibrarySynced:
                    SystemsContainer.Get<MainSystem>().Status = "Craft library synced";
                    SystemsContainer.Get<ChatSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendChatRequest();
                    MainSystem.NetworkState = ClientState.SyncingChat;
                    break;
                case ClientState.SyncingChat:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing chat";
                    break;
                case ClientState.ChatSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Chat synced";
                    SystemsContainer.Get<LockSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendLocksRequest();
                    MainSystem.NetworkState = ClientState.SyncingLocks;
                    break;
                case ClientState.SyncingLocks:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing locks";
                    break;
                case ClientState.LocksSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Locks synced";
                    SystemsContainer.Get<AdminSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendAdminsRequest();
                    MainSystem.NetworkState = ClientState.SyncingAdmins;
                    break;
                case ClientState.SyncingAdmins:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing admins";
                    break;
                case ClientState.AdminsSynced:
                    SystemsContainer.Get<MainSystem>().Status = "Admins synced";
                    SystemsContainer.Get<VesselLockSystem>().Enabled = true;
                    SystemsContainer.Get<VesselPositionSystem>().Enabled = true;
                    SystemsContainer.Get<VesselPositionAltSystem>().Enabled = true;
                    SystemsContainer.Get<VesselFlightStateSystem>().Enabled = true;
                    SystemsContainer.Get<VesselChangeSystem>().Enabled = true;
                    SystemsContainer.Get<VesselProtoSystem>().Enabled = true;
                    SystemsContainer.Get<VesselRemoveSystem>().Enabled = true;
                    SystemsContainer.Get<VesselImmortalSystem>().Enabled = true;
                    SystemsContainer.Get<VesselDockSystem>().Enabled = true;
                    SystemsContainer.Get<VesselSwitcherSystem>().Enabled = true;
                    NetworkSimpleMessageSender.SendGroupListRequest();
                    MainSystem.NetworkState = ClientState.SyncingGroups;
                    break;
                case ClientState.SyncingGroups:
                    SystemsContainer.Get<GroupSystem>().Enabled = true;
                    SystemsContainer.Get<MainSystem>().Status = "Syncing groups";
                    break;
                case ClientState.GroupsSynced:
                    NetworkSimpleMessageSender.SendVesselListRequest();
                    MainSystem.NetworkState = ClientState.SyncingVessels;
                    break;
                case ClientState.SyncingVessels:
                    SystemsContainer.Get<MainSystem>().Status = "Syncing vessels";
                    break;
                case ClientState.VesselsSynced:
                    LunaLog.Log("[LMP]: Vessels Synced!");
                    SystemsContainer.Get<MainSystem>().Status = "Syncing universe time";
                    MainSystem.NetworkState = ClientState.TimeLocking;
                    SystemsContainer.Get<FlagSystem>().Enabled = true;
                    SystemsContainer.Get<KerbalReassignerSystem>().Enabled = true;
                    SystemsContainer.Get<FlagSystem>().SendFlagList();
                    SystemsContainer.Get<PlayerColorSystem>().MessageSender.SendPlayerColorToServer();
                    break;
                case ClientState.TimeLocking:
                    if (SystemsContainer.Get<TimeSyncerSystem>().Synced)
                    {
                        LunaLog.Log("[LMP]: Time Locked!");
                        SystemsContainer.Get<MainSystem>().Status = "Starting game";
                        MainSystem.NetworkState = ClientState.TimeLocked;
                        SystemsContainer.Get<MainSystem>().StartGame = true;
                    }
                    break;
                case ClientState.TimeLocked:
                    MainSystem.NetworkState = ClientState.Starting;
                    break;
                case ClientState.Starting:
                    LunaLog.Log("[LMP]: All systems up and running! Poyekhali!!");
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        SystemsContainer.Get<MainSystem>().Status = "Running";

                        SystemsContainer.Get<MotdSystem>().Enabled = true;

                        MainSystem.NetworkState = ClientState.Running;

                        SystemsContainer.Get<GameSceneSystem>().Enabled = true;
                        SystemsContainer.Get<AsteroidSystem>().Enabled = true;
                        SystemsContainer.Get<ToolbarSystem>().Enabled = true;
                        NetworkSimpleMessageSender.SendMotdRequest();
                    }
                    break;
                case ClientState.Running:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (SystemsContainer.Get<MotdSystem>().DisplayMotd && HighLogic.LoadedScene != GameScenes.LOADING)
            {
                SystemsContainer.Get<MotdSystem>().DisplayMotd = false;
                SystemsContainer.Get<ScenarioSystem>().UpgradeTheAstronautComplexSoTheGameDoesntBugOut();
                ScreenMessages.PostScreenMessage(SystemsContainer.Get<MotdSystem>().ServerMotd, 10f, ScreenMessageStyle.UPPER_CENTER);
                //Control locks will bug out the space centre sceen, so remove them before starting.
                NetworkMain.DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway();
            }
        }

        #endregion

        private static void ShowDisconnectMessage()
        {
            if (HighLogic.LoadedScene < GameScenes.SPACECENTER)
                DisplayDisconnectMessage = false;

            if (DisplayDisconnectMessage)
            {
                ScreenMessages.PostScreenMessage("You have been disconnected!", 2f, ScreenMessageStyle.UPPER_CENTER);
            }
        }
    }
}