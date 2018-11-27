using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselOrbitDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public Vessel Vessel { get; set; }

        public OrbitDriver.UpdateMode ObtDriverMode { get; set; }

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            Vessel = vessel;
        }

        public VesselOrbitDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void PrintDisplay()
        {
            if (!Vessel) return;

            GUILayout.BeginHorizontal();

            StringBuilder.Length = 0;
            StringBuilder.Append("Update mode: ").AppendLine(ObtDriverMode.ToString());

            GUILayout.Label(StringBuilder.ToString());
            GUILayout.FlexibleSpace();
            switch (ObtDriverMode)
            {
                case OrbitDriver.UpdateMode.TRACK_Phys:
                    if (GUILayout.Button("Set as update"))
                    {
                        Vessel.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);
                    }
                    break;
                case OrbitDriver.UpdateMode.UPDATE:
                    if (GUILayout.Button("Set as track phys"))
                    {
                        Vessel.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.TRACK_Phys);
                    }
                    break;
            }

            GUILayout.EndHorizontal();

            StringBuilder.Length = 0;
            StringBuilder.AppendLine($"Semi major axis: {Vessel.orbit.semiMajorAxis}");
            StringBuilder.AppendLine($"Eccentricity: {Vessel.orbit.eccentricity}");
            StringBuilder.AppendLine($"Inclination: {Vessel.orbit.inclination}");
            StringBuilder.AppendLine($"LAN: {Vessel.orbit.LAN}");
            StringBuilder.AppendLine($"Arg Periapsis: {Vessel.orbit.argumentOfPeriapsis}");
            StringBuilder.AppendLine($"Mean anomaly: {Vessel.orbit.meanAnomaly}");
            StringBuilder.AppendLine($"Mean anomaly at Epoch: {Vessel.orbit.meanAnomalyAtEpoch}");
            StringBuilder.AppendLine($"Epoch: {Vessel.orbit.epoch}");
            StringBuilder.Append($"ObT: {Vessel.orbit.ObT}");

            GUILayout.Label(StringBuilder.ToString());
        }
    }
}
