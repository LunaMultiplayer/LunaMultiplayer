using LunaClient.Systems;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Facility;
using LunaClient.Systems.Flag;
using LunaClient.Systems.GameScene;
using LunaClient.Systems.Groups;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRangeSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselResourceSys;
using LunaClient.Systems.VesselStateSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using UnityEngine;

namespace LunaClient.Windows.Systems
{
    public partial class SystemsWindow
    {
        public void DrawContent(int windowId)
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
            CraftLibrary = GUILayout.Toggle(CraftLibrary, "Craft library system", ButtonStyle);
            if (CraftLibrary)
            {
                SystemsContainer.Get<CraftLibrarySystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<CraftLibrarySystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Facility = GUILayout.Toggle(Facility, "Facility system", ButtonStyle);
            if (Facility)
            {
                SystemsContainer.Get<FacilitySystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<FacilitySystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Flag = GUILayout.Toggle(Flag, "Flag system", ButtonStyle);
            if (Flag)
            {
                SystemsContainer.Get<FlagSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<FlagSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            GameScene = GUILayout.Toggle(GameScene, "GameScene system", ButtonStyle);
            if (GameScene)
            {
                SystemsContainer.Get<GameSceneSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<GameSceneSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Group = GUILayout.Toggle(Group, "Group system", ButtonStyle);
            if (Group)
            {
                SystemsContainer.Get<GroupSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<GroupSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Kerbal = GUILayout.Toggle(Kerbal, "Kerbal system", ButtonStyle);
            if (Kerbal)
            {
                SystemsContainer.Get<KerbalSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<KerbalSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Lock = GUILayout.Toggle(Lock, "Lock system", ButtonStyle);
            if (Lock)
            {
                SystemsContainer.Get<LockSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<LockSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            ModS = GUILayout.Toggle(ModS, "Mod system", ButtonStyle);
            if (ModS)
            {
                SystemsContainer.Get<ModSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<ModSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            //ModApi = GUILayout.Toggle(ModApi, "Mod api system", ButtonStyle);
            //if (ModApi)
            //{
            //    //This system cannot be toggled
            //    //SystemContainer.Get<ModApiSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<ModApiSystem>().Enabled, "ON/OFF", ButtonStyle);
            //}
            PlayerColor = GUILayout.Toggle(PlayerColor, "Player color system", ButtonStyle);
            if (PlayerColor)
            {
                SystemsContainer.Get<PlayerColorSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<PlayerColorSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            PlayerConnection = GUILayout.Toggle(PlayerConnection, "Player connection system", ButtonStyle);
            if (PlayerConnection)
            {
                SystemsContainer.Get<PlayerConnectionSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<PlayerConnectionSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Scenario = GUILayout.Toggle(Scenario, "Scenario system", ButtonStyle);
            if (Scenario)
            {
                //This system should never be toggled
                SystemsContainer.Get<ScenarioSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<ScenarioSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            TimeSyncer = GUILayout.Toggle(TimeSyncer, "Time sync system", ButtonStyle);
            if (TimeSyncer)
            {
                SystemsContainer.Get<TimeSyncerSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<TimeSyncerSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Toolbar = GUILayout.Toggle(Toolbar, "Toolbar system", ButtonStyle);
            if (Toolbar)
            {
                SystemsContainer.Get<ToolbarSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<ToolbarSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselDock = GUILayout.Toggle(VesselDock, "Vessel dock", ButtonStyle);
            if (VesselDock)
            {
                SystemsContainer.Get<VesselDockSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselDockSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselFlightState = GUILayout.Toggle(VesselFlightState, "Vessel flightstate", ButtonStyle);
            if (VesselFlightState)
            {
                SystemsContainer.Get<VesselFlightStateSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselFlightStateSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselImmortal = GUILayout.Toggle(VesselImmortal, "Vessel immortal", ButtonStyle);
            if (VesselImmortal)
            {
                SystemsContainer.Get<VesselImmortalSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselImmortalSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselLock = GUILayout.Toggle(VesselLock, "Vessel lock", ButtonStyle);
            if (VesselLock)
            {
                SystemsContainer.Get<VesselLockSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselLockSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselPosition = GUILayout.Toggle(VesselPosition, "Vessel position", ButtonStyle);
            if (VesselPosition)
            {
                SystemsContainer.Get<VesselPositionSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselPositionSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselUpdate = GUILayout.Toggle(VesselUpdate, "Vessel update", ButtonStyle);
            if (VesselUpdate)
            {
                SystemsContainer.Get<VesselUpdateSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselUpdateSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselResource = GUILayout.Toggle(VesselResource, "Vessel resources", ButtonStyle);
            if (VesselResource)
            {
                SystemsContainer.Get<VesselResourceSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselResourceSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselProto = GUILayout.Toggle(VesselProto, "Vessel proto", ButtonStyle);
            if (VesselProto)
            {
                SystemsContainer.Get<VesselProtoSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselProtoSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselRange = GUILayout.Toggle(VesselRange, "Vessel range", ButtonStyle);
            if (VesselRange)
            {
                SystemsContainer.Get<VesselRangeSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselRangeSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselState = GUILayout.Toggle(VesselState, "Vessel state", ButtonStyle);
            if (VesselState)
            {
                SystemsContainer.Get<VesselStateSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselStateSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselRemove = GUILayout.Toggle(VesselRemove, "Vessel remove", ButtonStyle);
            if (VesselRemove)
            {
                SystemsContainer.Get<VesselRemoveSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselRemoveSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            VesselSwitcher = GUILayout.Toggle(VesselSwitcher, "Vessel switcher", ButtonStyle);
            if (VesselSwitcher)
            {
                SystemsContainer.Get<VesselSwitcherSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselSwitcherSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
            Warp = GUILayout.Toggle(Warp, "Warp system", ButtonStyle);
            if (Warp)
            {
                //This system should never be toggled
                SystemsContainer.Get<WarpSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<WarpSystem>().Enabled, "ON/OFF", ButtonStyle);
            }
        }
    }
}
