using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.ExternalSeat;
using LunaClient.Systems.Facility;
using LunaClient.Systems.Flag;
using LunaClient.Systems.Groups;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.KscScene;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.ShareAchievements;
using LunaClient.Systems.ShareContracts;
using LunaClient.Systems.ShareFunds;
using LunaClient.Systems.ShareReputation;
using LunaClient.Systems.ShareScience;
using LunaClient.Systems.ShareScienceSubject;
using LunaClient.Systems.ShareStrategy;
using LunaClient.Systems.ShareTechnology;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselCrewSys;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFairingsSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPartSyncSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselResourceSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using UnityEngine;

namespace LunaClient.Windows.Systems
{
    public partial class SystemsWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth - 5), GUILayout.Height(WindowHeight - 100));
            
            PrintSystemButtons();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void PrintSystemButtons()
        {
            //Asteroid = GUILayout.Toggle(Asteroid, "Asteroid system", ButtonStyle);
            //if (Asteroid)
            //{
            //    //This system should never be toggled
            //    //SystemContainer.Get<AsteroidSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<AsteroidSystem>().Enabled, "ON/OFF", ButtonStyle);
            //}
            _craftLibrary = GUILayout.Toggle(_craftLibrary, "Craft library system", ButtonStyle);
            if (_craftLibrary)
            {
                CraftLibrarySystem.Singleton.Enabled = GUILayout.Toggle(CraftLibrarySystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _facility = GUILayout.Toggle(_facility, "Facility system", ButtonStyle);
            if (_facility)
            {
                FacilitySystem.Singleton.Enabled = GUILayout.Toggle(FacilitySystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _flag = GUILayout.Toggle(_flag, "Flag system", ButtonStyle);
            if (_flag)
            {
                FlagSystem.Singleton.Enabled = GUILayout.Toggle(FlagSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _externalSeat = GUILayout.Toggle(_externalSeat, "External seat", ButtonStyle);
            if (_externalSeat)
            {
                ExternalSeatSystem.Singleton.Enabled = GUILayout.Toggle(ExternalSeatSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _kscScene = GUILayout.Toggle(_kscScene, "KscScene system", ButtonStyle);
            if (_kscScene)
            {
                KscSceneSystem.Singleton.Enabled = GUILayout.Toggle(KscSceneSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _group = GUILayout.Toggle(_group, "Group system", ButtonStyle);
            if (_group)
            {
                GroupSystem.Singleton.Enabled = GUILayout.Toggle(GroupSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _kerbal = GUILayout.Toggle(_kerbal, "Kerbal system", ButtonStyle);
            if (_kerbal)
            {
                KerbalSystem.Singleton.Enabled = GUILayout.Toggle(KerbalSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _lock = GUILayout.Toggle(_lock, "Lock system", ButtonStyle);
            if (_lock)
            {
                LockSystem.Singleton.Enabled = GUILayout.Toggle(LockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _modS = GUILayout.Toggle(_modS, "Mod system", ButtonStyle);
            if (_modS)
            {
                ModSystem.Singleton.Enabled = GUILayout.Toggle(ModSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            //ModApi = GUILayout.Toggle(ModApi, "Mod api system", ButtonStyle);
            //if (ModApi)
            //{
            //    //This system cannot be toggled
            //    //SystemContainer.Get<ModApiSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<ModApiSystem>().Enabled, "ON/OFF", ButtonStyle);
            //}
            _playerColor = GUILayout.Toggle(_playerColor, "Player color system", ButtonStyle);
            if (_playerColor)
            {
                PlayerColorSystem.Singleton.Enabled = GUILayout.Toggle(PlayerColorSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _playerConnection = GUILayout.Toggle(_playerConnection, "Player connection system", ButtonStyle);
            if (_playerConnection)
            {
                PlayerConnectionSystem.Singleton.Enabled = GUILayout.Toggle(PlayerConnectionSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _scenario = GUILayout.Toggle(_scenario, "Scenario system", ButtonStyle);
            if (_scenario)
            {
                //This system should never be toggled
                ScenarioSystem.Singleton.Enabled = GUILayout.Toggle(ScenarioSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _timeSyncer = GUILayout.Toggle(_timeSyncer, "Time sync system", ButtonStyle);
            if (_timeSyncer)
            {
                TimeSyncerSystem.Singleton.Enabled = GUILayout.Toggle(TimeSyncerSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            //Toolbar = GUILayout.Toggle(Toolbar, "Toolbar system", ButtonStyle);
            //if (Toolbar)
            //{
            //    ToolbarSystem.Singleton.Enabled = GUILayout.Toggle(ToolbarSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            //}
            _vesselCrew = GUILayout.Toggle(_vesselCrew, "Vessel crew", ButtonStyle);
            if (_vesselCrew)
            {
                VesselCrewSystem.Singleton.Enabled = GUILayout.Toggle(VesselCrewSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselDock = GUILayout.Toggle(_vesselDock, "Vessel dock", ButtonStyle);
            if (_vesselDock)
            {
                VesselDockSystem.Singleton.Enabled = GUILayout.Toggle(VesselDockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselFlightState = GUILayout.Toggle(_vesselFlightState, "Vessel flightstate", ButtonStyle);
            if (_vesselFlightState)
            {
                VesselFlightStateSystem.Singleton.Enabled = GUILayout.Toggle(VesselFlightStateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselImmortal = GUILayout.Toggle(_vesselImmortal, "Vessel immortal", ButtonStyle);
            if (_vesselImmortal)
            {
                VesselImmortalSystem.Singleton.Enabled = GUILayout.Toggle(VesselImmortalSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselLock = GUILayout.Toggle(_vesselLock, "Vessel lock", ButtonStyle);
            if (_vesselLock)
            {
                VesselLockSystem.Singleton.Enabled = GUILayout.Toggle(VesselLockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselPosition = GUILayout.Toggle(_vesselPosition, "Vessel position", ButtonStyle);
            if (_vesselPosition)
            {
                VesselPositionSystem.Singleton.Enabled = GUILayout.Toggle(VesselPositionSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselUpdate = GUILayout.Toggle(_vesselUpdate, "Vessel update", ButtonStyle);
            if (_vesselUpdate)
            {
                VesselUpdateSystem.Singleton.Enabled = GUILayout.Toggle(VesselUpdateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselPartSync = GUILayout.Toggle(_vesselPartSync, "Vessel part method sync", ButtonStyle);
            if (_vesselPartSync)
            {
                VesselPartSyncSystem.Singleton.Enabled = GUILayout.Toggle(VesselPartSyncSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselResource = GUILayout.Toggle(_vesselResource, "Vessel resources", ButtonStyle);
            if (_vesselResource)
            {
                VesselResourceSystem.Singleton.Enabled = GUILayout.Toggle(VesselResourceSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselFairing = GUILayout.Toggle(_vesselFairing, "Vessel fairing", ButtonStyle);
            if (_vesselFairing)
            {
                VesselFairingsSystem.Singleton.Enabled = GUILayout.Toggle(VesselFairingsSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselProto = GUILayout.Toggle(_vesselProto, "Vessel proto", ButtonStyle);
            if (_vesselProto)
            {
                VesselProtoSystem.Singleton.Enabled = GUILayout.Toggle(VesselProtoSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselRemove = GUILayout.Toggle(_vesselRemove, "Vessel remove", ButtonStyle);
            if (_vesselRemove)
            {
                VesselRemoveSystem.Singleton.Enabled = GUILayout.Toggle(VesselRemoveSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _vesselSwitcher = GUILayout.Toggle(_vesselSwitcher, "Vessel switcher", ButtonStyle);
            if (_vesselSwitcher)
            {
                VesselSwitcherSystem.Singleton.Enabled = GUILayout.Toggle(VesselSwitcherSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _warp = GUILayout.Toggle(_warp, "Warp system", ButtonStyle);
            if (_warp)
            {
                //This system should never be toggled
                WarpSystem.Singleton.Enabled = GUILayout.Toggle(WarpSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareFunds = GUILayout.Toggle(_shareFunds, "Share Funds system", ButtonStyle);
            if (_shareFunds)
            {
                ShareFundsSystem.Singleton.Enabled = GUILayout.Toggle(ShareFundsSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareScience = GUILayout.Toggle(_shareScience, "Share Science system", ButtonStyle);
            if (_shareScience)
            {
                ShareScienceSystem.Singleton.Enabled = GUILayout.Toggle(ShareScienceSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareScienceSubject = GUILayout.Toggle(_shareScienceSubject, "Share Science subject system", ButtonStyle);
            if (_shareScienceSubject)
            {
                ShareScienceSubjectSystem.Singleton.Enabled = GUILayout.Toggle(ShareScienceSubjectSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareReputation = GUILayout.Toggle(_shareReputation, "Share Reputation system", ButtonStyle);
            if (_shareReputation)
            {
                ShareReputationSystem.Singleton.Enabled = GUILayout.Toggle(ShareReputationSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareTechnology = GUILayout.Toggle(_shareTechnology, "Share Technology system", ButtonStyle);
            if (_shareTechnology)
            {
                ShareTechnologySystem.Singleton.Enabled = GUILayout.Toggle(ShareTechnologySystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareContracts = GUILayout.Toggle(_shareContracts, "Share Contract system", ButtonStyle);
            if (_shareContracts)
            {
                ShareContractsSystem.Singleton.Enabled = GUILayout.Toggle(ShareContractsSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareAchievements = GUILayout.Toggle(_shareAchievements, "Share Achievements system", ButtonStyle);
            if (_shareAchievements)
            {
                ShareAchievementsSystem.Singleton.Enabled = GUILayout.Toggle(ShareAchievementsSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            _shareStrategy = GUILayout.Toggle(_shareStrategy, "Share Strategy system", ButtonStyle);
            if (_shareStrategy)
            {
                ShareStrategySystem.Singleton.Enabled = GUILayout.Toggle(ShareStrategySystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
        }
    }
}
