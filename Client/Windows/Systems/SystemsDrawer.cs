using LunaClient.Systems.CraftLibrary;
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
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Toolbar;
using LunaClient.Systems.VesselDockSys;
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
                CraftLibrarySystem.Singleton.Enabled = GUILayout.Toggle(CraftLibrarySystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Facility = GUILayout.Toggle(Facility, "Facility system", ButtonStyle);
            if (Facility)
            {
                FacilitySystem.Singleton.Enabled = GUILayout.Toggle(FacilitySystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Flag = GUILayout.Toggle(Flag, "Flag system", ButtonStyle);
            if (Flag)
            {
                FlagSystem.Singleton.Enabled = GUILayout.Toggle(FlagSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            KscScene = GUILayout.Toggle(KscScene, "KscScene system", ButtonStyle);
            if (KscScene)
            {
                KscSceneSystem.Singleton.Enabled = GUILayout.Toggle(KscSceneSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Group = GUILayout.Toggle(Group, "Group system", ButtonStyle);
            if (Group)
            {
                GroupSystem.Singleton.Enabled = GUILayout.Toggle(GroupSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Kerbal = GUILayout.Toggle(Kerbal, "Kerbal system", ButtonStyle);
            if (Kerbal)
            {
                KerbalSystem.Singleton.Enabled = GUILayout.Toggle(KerbalSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Lock = GUILayout.Toggle(Lock, "Lock system", ButtonStyle);
            if (Lock)
            {
                LockSystem.Singleton.Enabled = GUILayout.Toggle(LockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            ModS = GUILayout.Toggle(ModS, "Mod system", ButtonStyle);
            if (ModS)
            {
                ModSystem.Singleton.Enabled = GUILayout.Toggle(ModSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
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
                PlayerColorSystem.Singleton.Enabled = GUILayout.Toggle(PlayerColorSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            PlayerConnection = GUILayout.Toggle(PlayerConnection, "Player connection system", ButtonStyle);
            if (PlayerConnection)
            {
                PlayerConnectionSystem.Singleton.Enabled = GUILayout.Toggle(PlayerConnectionSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Scenario = GUILayout.Toggle(Scenario, "Scenario system", ButtonStyle);
            if (Scenario)
            {
                //This system should never be toggled
                ScenarioSystem.Singleton.Enabled = GUILayout.Toggle(ScenarioSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            TimeSyncer = GUILayout.Toggle(TimeSyncer, "Time sync system", ButtonStyle);
            if (TimeSyncer)
            {
                TimeSyncerSystem.Singleton.Enabled = GUILayout.Toggle(TimeSyncerSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Toolbar = GUILayout.Toggle(Toolbar, "Toolbar system", ButtonStyle);
            if (Toolbar)
            {
                ToolbarSystem.Singleton.Enabled = GUILayout.Toggle(ToolbarSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselDock = GUILayout.Toggle(VesselDock, "Vessel dock", ButtonStyle);
            if (VesselDock)
            {
                VesselDockSystem.Singleton.Enabled = GUILayout.Toggle(VesselDockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselFlightState = GUILayout.Toggle(VesselFlightState, "Vessel flightstate", ButtonStyle);
            if (VesselFlightState)
            {
                VesselFlightStateSystem.Singleton.Enabled = GUILayout.Toggle(VesselFlightStateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselImmortal = GUILayout.Toggle(VesselImmortal, "Vessel immortal", ButtonStyle);
            if (VesselImmortal)
            {
                VesselImmortalSystem.Singleton.Enabled = GUILayout.Toggle(VesselImmortalSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselLock = GUILayout.Toggle(VesselLock, "Vessel lock", ButtonStyle);
            if (VesselLock)
            {
                VesselLockSystem.Singleton.Enabled = GUILayout.Toggle(VesselLockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselPosition = GUILayout.Toggle(VesselPosition, "Vessel position", ButtonStyle);
            if (VesselPosition)
            {
                VesselPositionSystem.Singleton.Enabled = GUILayout.Toggle(VesselPositionSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselUpdate = GUILayout.Toggle(VesselUpdate, "Vessel update", ButtonStyle);
            if (VesselUpdate)
            {
                VesselUpdateSystem.Singleton.Enabled = GUILayout.Toggle(VesselUpdateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselPartSync = GUILayout.Toggle(VesselPartSync, "Vessel part sync", ButtonStyle);
            if (VesselPartSync)
            {
                VesselPartModuleSyncSystem.Singleton.Enabled = GUILayout.Toggle(VesselPartModuleSyncSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselResource = GUILayout.Toggle(VesselResource, "Vessel resources", ButtonStyle);
            if (VesselResource)
            {
                VesselResourceSystem.Singleton.Enabled = GUILayout.Toggle(VesselResourceSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselProto = GUILayout.Toggle(VesselProto, "Vessel proto", ButtonStyle);
            if (VesselProto)
            {
                VesselProtoSystem.Singleton.Enabled = GUILayout.Toggle(VesselProtoSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselPrecalc = GUILayout.Toggle(VesselPrecalc, "Vessel precalc", ButtonStyle);
            if (VesselPrecalc)
            {
                VesselPrecalcSystem.Singleton.Enabled = GUILayout.Toggle(VesselPrecalcSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselState = GUILayout.Toggle(VesselState, "Vessel state", ButtonStyle);
            if (VesselState)
            {
                VesselStateSystem.Singleton.Enabled = GUILayout.Toggle(VesselStateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselRemove = GUILayout.Toggle(VesselRemove, "Vessel remove", ButtonStyle);
            if (VesselRemove)
            {
                VesselRemoveSystem.Singleton.Enabled = GUILayout.Toggle(VesselRemoveSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            VesselSwitcher = GUILayout.Toggle(VesselSwitcher, "Vessel switcher", ButtonStyle);
            if (VesselSwitcher)
            {
                VesselSwitcherSystem.Singleton.Enabled = GUILayout.Toggle(VesselSwitcherSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
            Warp = GUILayout.Toggle(Warp, "Warp system", ButtonStyle);
            if (Warp)
            {
                //This system should never be toggled
                WarpSystem.Singleton.Enabled = GUILayout.Toggle(WarpSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
            }
        }
    }
}
