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
            DisplayFast = GUILayout.Toggle(DisplayFast, "Fast debug update", ButtonStyle);
            if (GUILayout.Button("Reset Profiler history", ButtonStyle))
            {
                VesselChangeSystem.Singleton.ResetProfilers();;
                VesselDockSystem.Singleton.ResetProfilers();
                VesselFlightStateSystem.Singleton.ResetProfilers();
                VesselImmortalSystem.Singleton.ResetProfilers();
                VesselLockSystem.Singleton.ResetProfilers();
                VesselPositionSystem.Singleton.ResetProfilers();
                VesselProtoSystem.Singleton.ResetProfilers();
                VesselRangeSystem.Singleton.ResetProfilers();
                VesselRemoveSystem.Singleton.ResetProfilers();
                VesselUpdateSystem.Singleton.ResetProfilers();
                CraftLibrarySystem.Singleton.ResetProfilers();
                FlagSystem.Singleton.ResetProfilers();
                ScenarioSystem.Singleton.ResetProfilers();
                TimeSyncerSystem.Singleton.ResetProfilers();
                ModApiSystem.Singleton.ResetProfilers();
                LockSystem.Singleton.ResetProfilers();
                KerbalSystem.Singleton.ResetProfilers();
                WarpSystem.Singleton.ResetProfilers();
            }
            GUILayout.EndHorizontal();

            CraftLibrary = GUILayout.Toggle(CraftLibrary, "Craft library system", ButtonStyle);
            if (CraftLibrary)
            {
                CraftLibrarySystem.Singleton.Enabled = GUILayout.Toggle(CraftLibrarySystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(CraftLibraryProfilerText))
                    GUILayout.Label(CraftLibraryProfilerText, LabelStyle);
            }
            Flag = GUILayout.Toggle(Flag, "Flag system", ButtonStyle);
            if (Flag)
            {
                FlagSystem.Singleton.Enabled = GUILayout.Toggle(FlagSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(FlagProfilerText))
                    GUILayout.Label(FlagProfilerText, LabelStyle);
            }
            Scenario = GUILayout.Toggle(Scenario, "Scenario system", ButtonStyle);
            if (Scenario)
            {
                //This system should never be toggled
                //ScenarioSystem.Singleton.Enabled = GUILayout.Toggle(ScenarioSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(ScenarioProfilerText))
                    GUILayout.Label(ScenarioProfilerText, LabelStyle);
            }
            TimeSyncer = GUILayout.Toggle(TimeSyncer, "Time sync system", ButtonStyle);
            if (TimeSyncer)
            {
                TimeSyncerSystem.Singleton.Enabled = GUILayout.Toggle(TimeSyncerSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(TimeSyncerProfilerText))
                    GUILayout.Label(TimeSyncerProfilerText, LabelStyle);
            }
            ModApi = GUILayout.Toggle(ModApi, "Mod api system", ButtonStyle);
            if (ModApi)
            {
                //This system cannot be toggled
                //ModApiSystem.Singleton.Enabled = GUILayout.Toggle(ModApiSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(ModApiProfilerText))
                    GUILayout.Label(ModApiProfilerText, LabelStyle);
            }
            Lock = GUILayout.Toggle(Lock, "Lock system", ButtonStyle);
            if (Lock)
            {
                LockSystem.Singleton.Enabled = GUILayout.Toggle(LockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(LockProfilerText))
                    GUILayout.Label(LockProfilerText, LabelStyle);
            }
            Kerbal = GUILayout.Toggle(Kerbal, "Kerbal system", ButtonStyle);
            if (Kerbal)
            {
                KerbalSystem.Singleton.Enabled = GUILayout.Toggle(KerbalSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(KerbalProfilerText))
                    GUILayout.Label(KerbalProfilerText, LabelStyle);
            }
            Warp = GUILayout.Toggle(Warp, "Warp system", ButtonStyle);
            if (Warp)
            {
                //This system should never be toggled
                //WarpSystem.Singleton.Enabled = GUILayout.Toggle(WarpSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                if (!string.IsNullOrEmpty(WarpProfilerText))
                    GUILayout.Label(WarpProfilerText, LabelStyle);
            }
            VesselSystems = GUILayout.Toggle(VesselSystems, "Vessel systems", ButtonStyle);
            if (VesselSystems)
            {
                VesselChange = GUILayout.Toggle(VesselChange, "Vessel change", ButtonStyle);
                if (VesselChange)
                {
                    VesselChangeSystem.Singleton.Enabled = GUILayout.Toggle(VesselChangeSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselChangeProfilerText))
                        GUILayout.Label(VesselChangeProfilerText, LabelStyle);
                }

                VesselDock = GUILayout.Toggle(VesselDock, "Vessel dock", ButtonStyle);
                if (VesselDock)
                {
                    VesselDockSystem.Singleton.Enabled = GUILayout.Toggle(VesselDockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselDockProfilerText))
                        GUILayout.Label(VesselDockProfilerText, LabelStyle);
                }

                VesselFlightState = GUILayout.Toggle(VesselFlightState, "Vessel flightstate", ButtonStyle);
                if (VesselFlightState)
                {
                    VesselFlightStateSystem.Singleton.Enabled = GUILayout.Toggle(VesselFlightStateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselFlightStateProfilerText))
                        GUILayout.Label(VesselFlightStateProfilerText, LabelStyle);
                }

                VesselImmortal = GUILayout.Toggle(VesselImmortal, "Vessel immortal", ButtonStyle);
                if (VesselImmortal)
                {
                    VesselImmortalSystem.Singleton.Enabled = GUILayout.Toggle(VesselImmortalSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselImmortalProfilerText))
                        GUILayout.Label(VesselImmortalProfilerText, LabelStyle);
                }

                VesselLock = GUILayout.Toggle(VesselLock, "Vessel lock", ButtonStyle);
                if (VesselLock)
                {
                    VesselLockSystem.Singleton.Enabled = GUILayout.Toggle(VesselLockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselLockProfilerText))
                        GUILayout.Label(VesselLockProfilerText, LabelStyle);
                }

                VesselPosition = GUILayout.Toggle(VesselPosition, "Vessel position", ButtonStyle);
                if (VesselPosition)
                {
                    VesselPositionSystem.Singleton.Enabled = GUILayout.Toggle(VesselPositionSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselPositionProfilerText))
                        GUILayout.Label(VesselPositionProfilerText, LabelStyle);
                }

                VesselProto = GUILayout.Toggle(VesselProto, "Vessel proto", ButtonStyle);
                if (VesselProto)
                {
                    VesselProtoSystem.Singleton.Enabled = GUILayout.Toggle(VesselProtoSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselProtoProfilerText))
                        GUILayout.Label(VesselProtoProfilerText, LabelStyle);
                }

                VesselRange = GUILayout.Toggle(VesselRange, "Vessel range", ButtonStyle);
                if (VesselRange)
                {
                    VesselRangeSystem.Singleton.Enabled = GUILayout.Toggle(VesselRangeSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselRangeProfilerText))
                        GUILayout.Label(VesselRangeProfilerText, LabelStyle);
                }

                VesselRemove = GUILayout.Toggle(VesselRemove, "Vessel remove", ButtonStyle);
                if (VesselRemove)
                {
                    VesselRemoveSystem.Singleton.Enabled = GUILayout.Toggle(VesselRemoveSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselRemoveProfilerText))
                        GUILayout.Label(VesselRemoveProfilerText, LabelStyle);
                }

                VesselUpdate = GUILayout.Toggle(VesselUpdate, "Vessel update", ButtonStyle);
                if (VesselUpdate)
                {
                    VesselUpdateSystem.Singleton.Enabled = GUILayout.Toggle(VesselUpdateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    if (!string.IsNullOrEmpty(VesselUpdateProfilerText))
                        GUILayout.Label(VesselUpdateProfilerText, LabelStyle);
                }
            }
            GUILayout.EndVertical();
        }
    }
}
