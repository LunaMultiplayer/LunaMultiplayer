using System.Diagnostics;
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
using UnityEngine;

namespace LunaClient.Windows.Systems
{
    public partial class SystemsWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DisplayFast = GUILayout.Toggle(DisplayFast, "Fast debug update", ButtonStyle);
            GUILayout.Label("Timer resolution: " + Stopwatch.Frequency + " hz", LabelStyle);
            if (GUILayout.Button("Reset Profiler history", ButtonStyle))
            {
                VesselChangeSystem.Singleton.UpdateProfiler.Reset();
                VesselDockSystem.Singleton.UpdateProfiler.Reset();
                VesselFlightStateSystem.Singleton.UpdateProfiler.Reset();
                VesselImmortalSystem.Singleton.UpdateProfiler.Reset();
                VesselLockSystem.Singleton.UpdateProfiler.Reset();
                VesselPositionSystem.Singleton.UpdateProfiler.Reset();
                VesselProtoSystem.Singleton.UpdateProfiler.Reset();
                VesselRangeSystem.Singleton.UpdateProfiler.Reset();
                VesselRemoveSystem.Singleton.UpdateProfiler.Reset();
                VesselUpdateSystem.Singleton.UpdateProfiler.Reset();
            }
            VesselSystems = GUILayout.Toggle(VesselSystems, "Vessel systems", ButtonStyle);
            if (VesselSystems)
            {
                VesselChange = GUILayout.Toggle(VesselChange, "Vessel change", ButtonStyle);
                if (VesselChange)
                {
                    VesselChangeSystem.Singleton.Enabled = GUILayout.Toggle(VesselChangeSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselChangeProfilerText, LabelStyle);
                }

                VesselDock = GUILayout.Toggle(VesselDock, "Vessel dock", ButtonStyle);
                if (VesselDock)
                {
                    VesselDockSystem.Singleton.Enabled = GUILayout.Toggle(VesselDockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselDockProfilerText, LabelStyle);
                }

                VesselFlightState = GUILayout.Toggle(VesselFlightState, "Vessel flightstate", ButtonStyle);
                if (VesselFlightState)
                {
                    VesselFlightStateSystem.Singleton.Enabled = GUILayout.Toggle(VesselFlightStateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselFlightStateProfilerText, LabelStyle);
                }

                VesselImmortal = GUILayout.Toggle(VesselImmortal, "Vessel immortal", ButtonStyle);
                if (VesselImmortal)
                {
                    VesselImmortalSystem.Singleton.Enabled = GUILayout.Toggle(VesselImmortalSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselImmortalProfilerText, LabelStyle);
                }

                VesselLock = GUILayout.Toggle(VesselLock, "Vessel lock", ButtonStyle);
                if (VesselLock)
                {
                    VesselLockSystem.Singleton.Enabled = GUILayout.Toggle(VesselLockSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselLockProfilerText, LabelStyle);
                }

                VesselPosition = GUILayout.Toggle(VesselPosition, "Vessel position", ButtonStyle);
                if (VesselPosition)
                {
                    VesselPositionSystem.Singleton.Enabled = GUILayout.Toggle(VesselPositionSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselPositionProfilerText, LabelStyle);
                }

                VesselProto = GUILayout.Toggle(VesselProto, "Vessel proto", ButtonStyle);
                if (VesselProto)
                {
                    VesselProtoSystem.Singleton.Enabled = GUILayout.Toggle(VesselProtoSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselProtoProfilerText, LabelStyle);
                }

                VesselRange = GUILayout.Toggle(VesselRange, "Vessel range", ButtonStyle);
                if (VesselRange)
                {
                    VesselRangeSystem.Singleton.Enabled = GUILayout.Toggle(VesselRangeSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselRangeProfilerText, LabelStyle);
                }

                VesselRemove = GUILayout.Toggle(VesselRemove, "Vessel remove", ButtonStyle);
                if (VesselRemove)
                {
                    VesselRemoveSystem.Singleton.Enabled = GUILayout.Toggle(VesselRemoveSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselRemoveProfilerText, LabelStyle);
                }

                VesselUpdate = GUILayout.Toggle(VesselUpdate, "Vessel update", ButtonStyle);
                if (VesselUpdate)
                {
                    VesselUpdateSystem.Singleton.Enabled = GUILayout.Toggle(VesselUpdateSystem.Singleton.Enabled, "ON/OFF", ButtonStyle);
                    GUILayout.Label(VesselUpdateProfilerText, LabelStyle);
                }
            }
            GUILayout.EndVertical();
        }
    }
}
