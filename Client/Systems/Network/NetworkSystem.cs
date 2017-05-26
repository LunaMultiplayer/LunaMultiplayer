using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Chat;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Flag;
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
using LunaClient.Systems.Warp;
using LunaCommon.Enums;
using System;
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
                case ClientState.Disconnected:
                case ClientState.Connecting:
                //Kill all systems while disconnected/connecting
                SystemsHandler.KillAllSystems();
                return;
                case ClientState.Connected:
                HandshakeSystem.Singleton.Enabled = true;
                break;
                case ClientState.Handshaking:
                MainSystem.Singleton.Status = "Connection successful, handshaking";
                break;
                case ClientState.Authenticated:
                PlayerConnectionSystem.Singleton.Enabled = true;
                StatusSystem.Singleton.Enabled = true;
                StatusSystem.Singleton.MessageSender.SendPlayerStatus(StatusSystem.Singleton.MyPlayerStatus);
                MainSystem.Singleton.NetworkState = ClientState.TimeSyncing;
                break;
                case ClientState.TimeSyncing:
                MainSystem.Singleton.Status = "Handshaking successful, syncing server clock";
                TimeSyncerSystem.Singleton.Enabled = true;
                if (TimeSyncerSystem.Singleton.Synced)
                    MainSystem.Singleton.NetworkState = ClientState.TimeSynced;
                else
                    TimeSyncerSystem.Singleton.MessageSender.SendTimeSyncRequest();
                break;
                case ClientState.TimeSynced:
                Debug.Log("[LMP]: Time Synced!");
                KerbalSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendKerbalsRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingKerbals;
                break;
                case ClientState.SyncingKerbals:
                MainSystem.Singleton.Status = "Syncing kerbals";
                break;
                case ClientState.KerbalsSynced:
                MainSystem.Singleton.Status = "Kerbals synced";
                SettingsSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendSettingsRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingSettings;
                break;
                case ClientState.SyncingSettings:
                MainSystem.Singleton.Status = "Syncing settings";
                break;
                case ClientState.SettingsSynced:
                MainSystem.Singleton.Status = "Settings synced";
                WarpSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendWarpSubspacesRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingWarpsubspaces;
                break;
                case ClientState.SyncingWarpsubspaces:
                MainSystem.Singleton.Status = "Syncing warp subspaces";
                break;
                case ClientState.WarpsubspacesSynced:
                MainSystem.Singleton.Status = "Warp subspaces synced";
                PlayerColorSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendColorsRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingColors;
                break;
                case ClientState.SyncingColors:
                MainSystem.Singleton.Status = "Syncing player colors";
                break;
                case ClientState.ColorsSynced:
                MainSystem.Singleton.Status = "Player colors synced";
                NetworkSimpleMessageSender.SendPlayersRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingPlayers;
                break;
                case ClientState.SyncingPlayers:
                MainSystem.Singleton.Status = "Syncing players";
                break;
                case ClientState.PlayersSynced:
                MainSystem.Singleton.Status = "Players synced";
                ScenarioSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendScenariosRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingScenarios;
                break;
                case ClientState.SyncingScenarios:
                MainSystem.Singleton.Status = "Syncing scenarios";
                break;
                case ClientState.ScneariosSynced:
                MainSystem.Singleton.Status = "Scenarios synced";
                CraftLibrarySystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendCraftLibraryRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingCraftlibrary;
                break;
                case ClientState.SyncingCraftlibrary:
                MainSystem.Singleton.Status = "Syncing craft library";
                break;
                case ClientState.CraftlibrarySynced:
                MainSystem.Singleton.Status = "Craft library synced";
                ChatSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendChatRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingChat;
                break;
                case ClientState.SyncingChat:
                MainSystem.Singleton.Status = "Syncing chat";
                break;
                case ClientState.ChatSynced:
                MainSystem.Singleton.Status = "Chat synced";
                LockSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendLocksRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingLocks;
                break;
                case ClientState.SyncingLocks:
                MainSystem.Singleton.Status = "Syncing locks";
                break;
                case ClientState.LocksSynced:
                MainSystem.Singleton.Status = "Locks synced";
                AdminSystem.Singleton.Enabled = true;
                NetworkSimpleMessageSender.SendAdminsRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingAdmins;
                break;
                case ClientState.SyncingAdmins:
                MainSystem.Singleton.Status = "Syncing admins";
                break;
                case ClientState.AdminsSynced:
                MainSystem.Singleton.Status = "Admins synced";
                VesselCommon.EnableAllSystems = true;
                NetworkSimpleMessageSender.SendVesselListRequest();
                MainSystem.Singleton.NetworkState = ClientState.SyncingVessels;
                break;
                case ClientState.SyncingVessels:
                MainSystem.Singleton.Status = "Syncing vessels";
                break;
                case ClientState.VesselsSynced:
                Debug.Log("[LMP]: Vessels Synced!");
                MainSystem.Singleton.Status = "Syncing universe time";
                MainSystem.Singleton.NetworkState = ClientState.TimeLocking;
                FlagSystem.Singleton.Enabled = true;
                KerbalReassignerSystem.Singleton.Enabled = true;
                FlagSystem.Singleton.SendFlagList();
                PlayerColorSystem.Singleton.MessageSender.SendPlayerColorToServer();
                break;
                case ClientState.TimeLocking:
                if (TimeSyncerSystem.Singleton.Synced)
                {
                    Debug.Log("[LMP]: Time Locked!");
                    MainSystem.Singleton.Status = "Starting game";
                    MainSystem.Singleton.NetworkState = ClientState.TimeLocked;
                    MainSystem.Singleton.StartGame = true;
                }
                break;
                case ClientState.TimeLocked:
                MainSystem.Singleton.NetworkState = ClientState.Starting;
                break;
                case ClientState.Starting:
                Debug.Log("[LMP]: All systems up and running! Poyekhali!!");
                if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                {
                    MainSystem.Singleton.Status = "Running";

                    MotdSystem.Singleton.Enabled = true;

                    MainSystem.Singleton.DisplayDisconnectMessage = false;
                    MainSystem.Singleton.NetworkState = ClientState.Running;

                    AsteroidSystem.Singleton.Enabled = true;
                    ToolbarSystem.Singleton.Enabled = true;
                    NetworkSimpleMessageSender.SendMotdRequest();
                }
                break;
                case ClientState.Running:
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