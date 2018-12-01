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
            ObtDriverMode = vessel.orbitDriver.updateMode;
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
            StringBuilder.AppendLine($"Semi major axis: {Vessel.orbit.semiMajorAxis}")
                .AppendLine($"Eccentricity: {Vessel.orbit.eccentricity}")
                .AppendLine($"Inclination: {Vessel.orbit.inclination}")
                .AppendLine($"LAN: {Vessel.orbit.LAN}")
                .AppendLine($"Arg Periapsis: {Vessel.orbit.argumentOfPeriapsis}")
                .AppendLine($"Mean anomaly: {Vessel.orbit.meanAnomaly}")
                .AppendLine($"Mean anomaly at Epoch: {Vessel.orbit.meanAnomalyAtEpoch}")
                .AppendLine($"Epoch: {Vessel.orbit.epoch}")
                .Append($"ObT: {Vessel.orbit.ObT}");

            GUILayout.Label(StringBuilder.ToString());
        }
    }
}
