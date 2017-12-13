using LunaClient.Systems;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Facility;
using LunaClient.Systems.Flag;
using LunaClient.Systems.GameScene;
using LunaClient.Systems.Groups;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Mod;
using LunaClient.Systems.ModApi;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.Status;
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
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
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
            DisplayFast = GUILayout.Toggle(DisplayFast, "Fast debug update", ButtonStyle);
            if (GUILayout.Button("Reset Profiler history", ButtonStyle))
            {
                LunaProfiler.FixedUpdateData.Reset();
                LunaProfiler.UpdateData.Reset();
                LunaProfiler.LateUpdateData.Reset();
                LunaProfiler.GuiData.Reset();

                SystemsContainer.Get<AsteroidSystem>().ResetProfilers();
                SystemsContainer.Get<CraftLibrarySystem>().ResetProfilers();
                SystemsContainer.Get<FacilitySystem>().ResetProfilers();
                SystemsContainer.Get<FlagSystem>().ResetProfilers();
                SystemsContainer.Get<GameSceneSystem>().ResetProfilers();
                SystemsContainer.Get<GroupSystem>().ResetProfilers();
                SystemsContainer.Get<KerbalSystem>().ResetProfilers();
                SystemsContainer.Get<LockSystem>().ResetProfilers();
                SystemsContainer.Get<ModSystem>().ResetProfilers();
                SystemsContainer.Get<ModApiSystem>().ResetProfilers();
                SystemsContainer.Get<PlayerColorSystem>().ResetProfilers();
                SystemsContainer.Get<PlayerConnectionSystem>().ResetProfilers();
                SystemsContainer.Get<ScenarioSystem>().ResetProfilers();
                SystemsContainer.Get<StatusSystem>().ResetProfilers();
                SystemsContainer.Get<TimeSyncerSystem>().ResetProfilers();
                SystemsContainer.Get<ToolbarSystem>().ResetProfilers();
                SystemsContainer.Get<VesselDockSystem>().ResetProfilers();
                SystemsContainer.Get<VesselFlightStateSystem>().ResetProfilers();
                SystemsContainer.Get<VesselImmortalSystem>().ResetProfilers();
                SystemsContainer.Get<VesselLockSystem>().ResetProfilers();
                SystemsContainer.Get<VesselPositionSystem>().ResetProfilers();
                SystemsContainer.Get<VesselProtoSystem>().ResetProfilers();
                SystemsContainer.Get<VesselRangeSystem>().ResetProfilers();
                SystemsContainer.Get<VesselRemoveSystem>().ResetProfilers();
                SystemsContainer.Get<VesselSwitcherSystem>().ResetProfilers();
                SystemsContainer.Get<WarpSystem>().ResetProfilers();
            }
            GUILayout.EndHorizontal();

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth - 5), GUILayout.Height(WindowHeight - 100));

            GUILayout.Label(LmpProfilerText, LabelStyle);
            PrintSystemButtons();

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void PrintSystemButtons()
        {
            Asteroid = GUILayout.Toggle(Asteroid, "Asteroid system", ButtonStyle);
            if (Asteroid)
            {
                //This system should never be toggled
                //SystemContainer.Get<AsteroidSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<AsteroidSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(AsteroidProfilerText))
                    GUILayout.Label(AsteroidProfilerText, LabelStyle);
            }
            CraftLibrary = GUILayout.Toggle(CraftLibrary, "Craft library system", ButtonStyle);
            if (CraftLibrary)
            {
                SystemsContainer.Get<CraftLibrarySystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<CraftLibrarySystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(CraftLibraryProfilerText))
                    GUILayout.Label(CraftLibraryProfilerText, LabelStyle);
            }
            Facility = GUILayout.Toggle(Facility, "Facility system", ButtonStyle);
            if (Facility)
            {
                SystemsContainer.Get<FacilitySystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<FacilitySystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(FacilityProfilerText))
                    GUILayout.Label(FacilityProfilerText, LabelStyle);
            }
            Flag = GUILayout.Toggle(Flag, "Flag system", ButtonStyle);
            if (Flag)
            {
                SystemsContainer.Get<FlagSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<FlagSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(FlagProfilerText))
                    GUILayout.Label(FlagProfilerText, LabelStyle);
            }
            GameScene = GUILayout.Toggle(GameScene, "GameScene system", ButtonStyle);
            if (GameScene)
            {
                SystemsContainer.Get<GameSceneSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<GameSceneSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(GameSceneProfilerText))
                    GUILayout.Label(GameSceneProfilerText, LabelStyle);
            }
            Group = GUILayout.Toggle(Group, "Group system", ButtonStyle);
            if (Group)
            {
                SystemsContainer.Get<GroupSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<GroupSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(GroupProfilerText))
                    GUILayout.Label(GroupProfilerText, LabelStyle);
            }
            Kerbal = GUILayout.Toggle(Kerbal, "Kerbal system", ButtonStyle);
            if (Kerbal)
            {
                SystemsContainer.Get<KerbalSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<KerbalSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(KerbalProfilerText))
                    GUILayout.Label(KerbalProfilerText, LabelStyle);
            }
            Lock = GUILayout.Toggle(Lock, "Lock system", ButtonStyle);
            if (Lock)
            {
                SystemsContainer.Get<LockSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<LockSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(LockProfilerText))
                    GUILayout.Label(LockProfilerText, LabelStyle);
            }
            ModS = GUILayout.Toggle(ModS, "Mod system", ButtonStyle);
            if (ModS)
            {
                SystemsContainer.Get<ModSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<ModSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(ModProfilerText))
                    GUILayout.Label(ModProfilerText, LabelStyle);
            }
            ModApi = GUILayout.Toggle(ModApi, "Mod api system", ButtonStyle);
            if (ModApi)
            {
                //This system cannot be toggled
                //SystemContainer.Get<ModApiSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<ModApiSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(ModApiProfilerText))
                    GUILayout.Label(ModApiProfilerText, LabelStyle);
            }
            PlayerColor = GUILayout.Toggle(PlayerColor, "Player color system", ButtonStyle);
            if (PlayerColor)
            {
                SystemsContainer.Get<PlayerColorSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<PlayerColorSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(PlayerColorProfilerText))
                    GUILayout.Label(PlayerColorProfilerText, LabelStyle);
            }
            PlayerConnection = GUILayout.Toggle(PlayerConnection, "Player connection system", ButtonStyle);
            if (PlayerConnection)
            {
                SystemsContainer.Get<PlayerConnectionSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<PlayerConnectionSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(PlayerConnectionProfilerText))
                    GUILayout.Label(PlayerConnectionProfilerText, LabelStyle);
            }
            Scenario = GUILayout.Toggle(Scenario, "Scenario system", ButtonStyle);
            if (Scenario)
            {
                //This system should never be toggled
                SystemsContainer.Get<ScenarioSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<ScenarioSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(ScenarioProfilerText))
                    GUILayout.Label(ScenarioProfilerText, LabelStyle);
            }
            TimeSyncer = GUILayout.Toggle(TimeSyncer, "Time sync system", ButtonStyle);
            if (TimeSyncer)
            {
                SystemsContainer.Get<TimeSyncerSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<TimeSyncerSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(TimeSyncerProfilerText))
                    GUILayout.Label(TimeSyncerProfilerText, LabelStyle);
            }
            Toolbar = GUILayout.Toggle(Toolbar, "Toolbar system", ButtonStyle);
            if (Toolbar)
            {
                SystemsContainer.Get<ToolbarSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<ToolbarSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(ToolbarProfilerText))
                    GUILayout.Label(ToolbarProfilerText, LabelStyle);
            }
            VesselDock = GUILayout.Toggle(VesselDock, "Vessel dock", ButtonStyle);
            if (VesselDock)
            {
                SystemsContainer.Get<VesselDockSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselDockSystem>().Enabled,
                    "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(VesselDockProfilerText))
                    GUILayout.Label(VesselDockProfilerText, LabelStyle);
            }
            VesselFlightState = GUILayout.Toggle(VesselFlightState, "Vessel flightstate", ButtonStyle);
            if (VesselFlightState)
            {
                SystemsContainer.Get<VesselFlightStateSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselFlightStateSystem>().Enabled,
                    "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(VesselFlightStateProfilerText))
                    GUILayout.Label(VesselFlightStateProfilerText, LabelStyle);
            }
            VesselImmortal = GUILayout.Toggle(VesselImmortal, "Vessel immortal", ButtonStyle);
            if (VesselImmortal)
            {
                SystemsContainer.Get<VesselImmortalSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselImmortalSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselImmortalProfilerText))
                    GUILayout.Label(VesselImmortalProfilerText, LabelStyle);
            }
            VesselLock = GUILayout.Toggle(VesselLock, "Vessel lock", ButtonStyle);
            if (VesselLock)
            {
                SystemsContainer.Get<VesselLockSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselLockSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselLockProfilerText))
                    GUILayout.Label(VesselLockProfilerText, LabelStyle);
            }
            VesselPosition = GUILayout.Toggle(VesselPosition, "Vessel position", ButtonStyle);
            if (VesselPosition)
            {
                SystemsContainer.Get<VesselPositionSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselPositionSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselPositionProfilerText))
                    GUILayout.Label(VesselPositionProfilerText, LabelStyle);
            }
            VesselProto = GUILayout.Toggle(VesselProto, "Vessel proto", ButtonStyle);
            if (VesselProto)
            {
                SystemsContainer.Get<VesselProtoSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselProtoSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselProtoProfilerText))
                    GUILayout.Label(VesselProtoProfilerText, LabelStyle);
            }
            VesselRange = GUILayout.Toggle(VesselRange, "Vessel range", ButtonStyle);
            if (VesselRange)
            {
                SystemsContainer.Get<VesselRangeSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselRangeSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselRangeProfilerText))
                    GUILayout.Label(VesselRangeProfilerText, LabelStyle);
            }
            VesselRemove = GUILayout.Toggle(VesselRemove, "Vessel remove", ButtonStyle);
            if (VesselRemove)
            {
                SystemsContainer.Get<VesselRemoveSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselRemoveSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselRemoveProfilerText))
                    GUILayout.Label(VesselRemoveProfilerText, LabelStyle);
            }
            VesselSwitcher = GUILayout.Toggle(VesselSwitcher, "Vessel switcher", ButtonStyle);
            if (VesselSwitcher)
            {
                SystemsContainer.Get<VesselSwitcherSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselSwitcherSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselSwitcherProfilerText))
                    GUILayout.Label(VesselSwitcherProfilerText, LabelStyle);
            }
            Warp = GUILayout.Toggle(Warp, "Warp system", ButtonStyle);
            if (Warp)
            {
                //This system should never be toggled
                SystemsContainer.Get<WarpSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<WarpSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(WarpProfilerText))
                    GUILayout.Label(WarpProfilerText, LabelStyle);
            }
        }
    }
}
