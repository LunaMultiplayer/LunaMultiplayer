using System;
using LunaClient.Base;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.ColorSystem;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Flag;
using LunaClient.Systems.KerbalReassigner;
using LunaClient.Systems.Motd;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.Warp;
using LunaCommon.Enums;
using LunaClient.Network;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Systems.Network
{
    public class NetworkSystem : System<NetworkSystem>
    {
        /// <summary>
        /// This system must be ALWAYS enabled!
        /// </summary>
        public override bool Enabled => true;

        #region Constructor

        public NetworkSystem()
        {
            //We setup the routine in the constructor as this system is always enabled
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, NetworkUpdate));
        }

        #endregion

        #region Update method

        private static void NetworkUpdate()
        {
            switch (MainSystem.Singleton.NetworkState)
            {
                case ClientState.DISCONNECTED:
                case ClientState.CONNECTING:
                    //Kill all systems while disconnected/connecting
                    SystemsHandler.KillAllSystems();
                    return;
                case ClientState.CONNECTED:
                    HandshakeSystem.Singleton.Enabled = true;
                    break;
                case ClientState.HANDSHAKING:
                    MainSystem.Singleton.Status = "Connection successful, handshaking";
                    break;
                case ClientState.AUTHENTICATED:
                    PlayerConnectionSystem.Singleton.Enabled = true;
                    StatusSystem.Singleton.Enabled = true;
                    StatusSystem.Singleton.MessageSender.SendPlayerStatus(StatusSystem.Singleton.MyPlayerStatus);
                    MainSystem.Singleton.NetworkState = ClientState.TIME_SYNCING;
                    break;
                case ClientState.TIME_SYNCING:
                    MainSystem.Singleton.Status = "Handshaking successful, syncing server clock";
                    TimeSyncerSystem.Singleton.Enabled = true;
                    if (TimeSyncerSystem.Singleton.Synced)
                        MainSystem.Singleton.NetworkState = ClientState.TIME_SYNCED;
                    else
                        TimeSyncerSystem.Singleton.MessageSender.SendTimeSyncRequest();
                    break;
                case ClientState.TIME_SYNCED:
                    Debug.Log("[LMP]: Time Synced!");
                    KerbalSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendKerbalsRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_KERBALS;
                    break;
                case ClientState.SYNCING_KERBALS:
                    MainSystem.Singleton.Status = "Syncing kerbals";
                    break;
                case ClientState.KERBALS_SYNCED:
                    MainSystem.Singleton.Status = "Kerbals synced";
                    SettingsSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendSettingsRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_SETTINGS;
                    break;
                case ClientState.SYNCING_SETTINGS:
                    MainSystem.Singleton.Status = "Syncing settings";
                    break;
                case ClientState.SETTINGS_SYNCED:
                    MainSystem.Singleton.Status = "Settings synced";
                    WarpSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendWarpSubspacesRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_WARPSUBSPACES;
                    break;
                case ClientState.SYNCING_WARPSUBSPACES:
                    MainSystem.Singleton.Status = "Syncing warp subspaces";
                    break;
                case ClientState.WARPSUBSPACES_SYNCED:
                    MainSystem.Singleton.Status = "Warp subspaces synced";
                    PlayerColorSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendColorsRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_COLORS;
                    break;
                case ClientState.SYNCING_COLORS:
                    MainSystem.Singleton.Status = "Syncing player colors";
                    break;
                case ClientState.COLORS_SYNCED:
                    MainSystem.Singleton.Status = "Player colors synced";
                    NetworkSimpleMessageSender.SendPlayersRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_PLAYERS;
                    break;
                case ClientState.SYNCING_PLAYERS:
                    MainSystem.Singleton.Status = "Syncing players";
                    break;
                case ClientState.PLAYERS_SYNCED:
                    MainSystem.Singleton.Status = "Players synced";
                    ScenarioSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendScenariosRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_SCENARIOS;
                    break;
                case ClientState.SYNCING_SCENARIOS:
                    MainSystem.Singleton.Status = "Syncing scenarios";
                    break;
                case ClientState.SCNEARIOS_SYNCED:
                    MainSystem.Singleton.Status = "Scenarios synced";
                    CraftLibrarySystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendCraftLibraryRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_CRAFTLIBRARY;
                    break;
                case ClientState.SYNCING_CRAFTLIBRARY:
                    MainSystem.Singleton.Status = "Syncing craft library";
                    break;
                case ClientState.CRAFTLIBRARY_SYNCED:
                    MainSystem.Singleton.Status = "Craft library synced";
                    ChatSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendChatRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_CHAT;
                    break;
                case ClientState.SYNCING_CHAT:
                    MainSystem.Singleton.Status = "Syncing chat";
                    break;
                case ClientState.CHAT_SYNCED:
                    MainSystem.Singleton.Status = "Chat synced";
                    LockSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendLocksRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_LOCKS;
                    break;
                case ClientState.SYNCING_LOCKS:
                    MainSystem.Singleton.Status = "Syncing locks";
                    break;
                case ClientState.LOCKS_SYNCED:
                    MainSystem.Singleton.Status = "Locks synced";
                    AdminSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendAdminsRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_ADMINS;
                    break;
                case ClientState.SYNCING_ADMINS:
                    MainSystem.Singleton.Status = "Syncing admins";
                    break;
                case ClientState.ADMINS_SYNCED:
                    MainSystem.Singleton.Status = "Admins synced";
                    VesselCommon.EnableAllSystems = true;
                    NetworkSimpleMessageSender.SendVesselListRequest();
                    MainSystem.Singleton.NetworkState = ClientState.SYNCING_VESSELS;
                    break;
                case ClientState.SYNCING_VESSELS:
                    MainSystem.Singleton.Status = "Syncing vessels";
                    break;
                case ClientState.VESSELS_SYNCED:
                    Debug.Log("[LMP]: Vessels Synced!");
                    MainSystem.Singleton.Status = "Syncing universe time";
                    MainSystem.Singleton.NetworkState = ClientState.TIME_LOCKING;
                    FlagSystem.Singleton.Enabled = true;
                    KerbalReassignerSystem.Singleton.Enabled = true;
                    FlagSystem.Singleton.SendFlagList();
                    PlayerColorSystem.Singleton.MessageSender.SendPlayerColorToServer();
                    break;
                case ClientState.TIME_LOCKING:
                    if (TimeSyncerSystem.Singleton.Synced)
                    {
                        Debug.Log("[LMP]: Time Locked!");
                        MainSystem.Singleton.Status = "Starting game";
                        MainSystem.Singleton.NetworkState = ClientState.TIME_LOCKED;
                        MainSystem.Singleton.StartGame = true;
                    }
                    break;
                case ClientState.TIME_LOCKED:
                    MainSystem.Singleton.NetworkState = ClientState.STARTING;
                    break;
                case ClientState.STARTING:
                    Debug.Log("[LMP]: All systems up and running! Poyekhali!!");
                    if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                    {
                        MainSystem.Singleton.Status = "Running";

                        MotdSystem.Singleton.Enabled = true;

                        MainSystem.Singleton.DisplayDisconnectMessage = false;
                        MainSystem.Singleton.NetworkState = ClientState.RUNNING;
                        
                        AsteroidSystem.Singleton.Enabled = true;
                        ToolbarSystem.Singleton.Enabled = true;
                        NetworkSimpleMessageSender.SendMotdRequest();
                    }
                    break;
                case ClientState.RUNNING:
                    MainSystem.Singleton.GameRunning = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (MotdSystem.Singleton.DisplayMotd && (HighLogic.LoadedScene != GameScenes.LOADING))
            {
                MotdSystem.Singleton.DisplayMotd = false;
                ScenarioSystem.Singleton.UpgradeTheAstronautComplexSoTheGameDoesntBugOut();
                ScreenMessages.PostScreenMessage(MotdSystem.Singleton.ServerMotd, 10f, ScreenMessageStyle.UPPER_CENTER);
                //Control locks will bug out the space centre sceen, so remove them before starting.
                NetworkMain.DeleteAllTheControlLocksSoTheSpaceCentreBugGoesAway();
            }
        }

        #endregion
    }
}