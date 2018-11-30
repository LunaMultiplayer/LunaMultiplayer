using LmpClient.Extensions;
using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{

    internal class VesselDataDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public Vessel Vessel { get; set; }

        public VesselDataDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            Vessel = vessel;
        }

        protected override void PrintDisplay()
        {
            if (!Vessel) return;

            GUILayout.Label($"Stage: {Vessel.currentStage}");
            GUILayout.Label($"Immortal: {Vessel.IsImmortal()}");
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Loaded: {Vessel.loaded}");
            GUILayout.FlexibleSpace();
            if (Vessel.loaded)
            {
                if (GUILayout.Button("Unload"))
                    FlightGlobals.FindVessel(VesselId).vesselRanges = UnloadRanges;
            }
            else
            {
                if (GUILayout.Button("Load"))
                    FlightGlobals.FindVessel(VesselId).vesselRanges = LoadRanges;
            }
            if (GUILayout.Button("Reset"))
                FlightGlobals.FindVessel(VesselId).vesselRanges = PhysicsGlobals.Instance.VesselRangesDefault;
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Packed: {Vessel.packed}");
            GUILayout.FlexibleSpace();
            if (Vessel.packed)
            {
                if (GUILayout.Button("Unpack"))
                    FlightGlobals.FindVessel(VesselId).vesselRanges = UnPackRanges;
            }
            else
            {
                if (GUILayout.Button("Pack"))
                    FlightGlobals.FindVessel(VesselId).vesselRanges = PackRanges;
            }
            if (GUILayout.Button("Reset"))
                FlightGlobals.FindVessel(VesselId).vesselRanges = PhysicsGlobals.Instance.VesselRangesDefault;
            GUILayout.EndHorizontal();
        }

        #region Ranges

        public static VesselRanges PackRanges { get; } = new VesselRanges
        {
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping) { pack = 0, unpack = int.MaxValue },
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying) { pack = 0, unpack = int.MaxValue },
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed) { pack = 0, unpack = int.MaxValue },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = 0, unpack = int.MaxValue },
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = 0, unpack = int.MaxValue },
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = 0, unpack = int.MaxValue },
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = 0, unpack = int.MaxValue }
        };

        public static VesselRanges UnPackRanges { get; } = new VesselRanges
        {
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping) { pack = int.MaxValue, unpack = 0 },
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying) { pack = int.MaxValue, unpack = 0 },
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed) { pack = int.MaxValue, unpack = 0 },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = int.MaxValue, unpack = 0 },
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = int.MaxValue, unpack = 0 },
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = int.MaxValue, unpack = 0 },
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { pack = int.MaxValue, unpack = 0 }
        };

        public static VesselRanges LoadRanges { get; } = new VesselRanges
        {
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping) { load = 0, unload = int.MaxValue },
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying) { load = 0, unload = int.MaxValue },
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed) { load = 0, unload = int.MaxValue },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = 0, unload = int.MaxValue },
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = 0, unload = int.MaxValue },
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = 0, unload = int.MaxValue },
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = 0, unload = int.MaxValue }
        };

        public static VesselRanges UnloadRanges { get; } = new VesselRanges
        {
            escaping = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.escaping) { load = int.MaxValue, unload = 0 },
            flying = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.flying) { load = int.MaxValue, unload = 0 },
            landed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.landed) { load = int.MaxValue, unload = 0 },
            orbit = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = int.MaxValue, unload = 0 },
            prelaunch = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = int.MaxValue, unload = 0 },
            splashed = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = int.MaxValue, unload = 0 },
            subOrbital = new VesselRanges.Situation(PhysicsGlobals.Instance.VesselRangesDefault.orbit) { load = int.MaxValue, unload = 0 }
        };

        #endregion
    }
}
