using LunaClient.Systems;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Flag;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.ModApi;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselChangeSys;
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
                SystemsContainer.Get<FlagSystem>().ResetProfilers();
                SystemsContainer.Get<ScenarioSystem>().ResetProfilers();
                SystemsContainer.Get<TimeSyncerSystem>().ResetProfilers();
                SystemsContainer.Get<ModApiSystem>().ResetProfilers();
                SystemsContainer.Get<LockSystem>().ResetProfilers();
                SystemsContainer.Get<KerbalSystem>().ResetProfilers();
                SystemsContainer.Get<VesselDockSystem>().ResetProfilers();
                SystemsContainer.Get<VesselSwitcherSystem>().ResetProfilers();
                SystemsContainer.Get<VesselFlightStateSystem>().ResetProfilers();
                SystemsContainer.Get<VesselRangeSystem>().ResetProfilers();
                SystemsContainer.Get<VesselImmortalSystem>().ResetProfilers();
                SystemsContainer.Get<VesselLockSystem>().ResetProfilers();
                SystemsContainer.Get<VesselPositionSystem>().ResetProfilers();
                SystemsContainer.Get<VesselProtoSystem>().ResetProfilers();
                SystemsContainer.Get<VesselRemoveSystem>().ResetProfilers();
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
            Flag = GUILayout.Toggle(Flag, "Flag system", ButtonStyle);
            if (Flag)
            {
                SystemsContainer.Get<FlagSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<FlagSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(FlagProfilerText))
                    GUILayout.Label(FlagProfilerText, LabelStyle);
            }
            Scenario = GUILayout.Toggle(Scenario, "Scenario system", ButtonStyle);
            if (Scenario)
            {
                //This system should never be toggled
                //SystemContainer.Get<ScenarioSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<ScenarioSystem>().Enabled, "ON/OFF", ButtonStyle);
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
            ModApi = GUILayout.Toggle(ModApi, "Mod api system", ButtonStyle);
            if (ModApi)
            {
                //This system cannot be toggled
                //SystemContainer.Get<ModApiSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<ModApiSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(ModApiProfilerText))
                    GUILayout.Label(ModApiProfilerText, LabelStyle);
            }
            Kerbal = GUILayout.Toggle(Kerbal, "Kerbal system", ButtonStyle);
            if (Kerbal)
            {
                SystemsContainer.Get<KerbalSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<KerbalSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(KerbalProfilerText))
                    GUILayout.Label(KerbalProfilerText, LabelStyle);
            }
            Warp = GUILayout.Toggle(Warp, "Warp system", ButtonStyle);
            if (Warp)
            {
                //This system should never be toggled
                //SystemContainer.Get<WarpSystem>().Enabled = GUILayout.Toggle(SystemContainer.Get<WarpSystem>().Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(WarpProfilerText))
                    GUILayout.Label(WarpProfilerText, LabelStyle);
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

            VesselChange = GUILayout.Toggle(VesselChange, "Vessel change", ButtonStyle);
            if (VesselChange)
            {
                SystemsContainer.Get<VesselChangeSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselChangeSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselChangeProfilerText))
                    GUILayout.Label(VesselChangeProfilerText, LabelStyle);
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

            VesselRemove = GUILayout.Toggle(VesselRemove, "Vessel remove", ButtonStyle);
            if (VesselRemove)
            {
                SystemsContainer.Get<VesselRemoveSystem>().Enabled = GUILayout.Toggle(SystemsContainer.Get<VesselRemoveSystem>().Enabled, "ON/OFF",
                    ButtonStyle);
                if (!string.IsNullOrEmpty(VesselRemoveProfilerText))
                    GUILayout.Label(VesselRemoveProfilerText, LabelStyle);
            }
        }
    }
}
