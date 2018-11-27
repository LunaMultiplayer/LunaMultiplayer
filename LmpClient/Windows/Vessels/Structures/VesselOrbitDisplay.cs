using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselOrbitDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public Orbit Orbit { get; set; } = new Orbit();

        public OrbitDriver.UpdateMode ObtDriverMode { get; set; }

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            ObtDriverMode = vessel.orbitDriver.updateMode;
            Orbit.semiMajorAxis = vessel.orbit.semiMajorAxis;
            Orbit.eccentricity = vessel.orbit.eccentricity;
            Orbit.inclination = vessel.orbit.inclination;
            Orbit.LAN = vessel.orbit.LAN;
            Orbit.argumentOfPeriapsis = vessel.orbit.argumentOfPeriapsis;
            Orbit.meanAnomaly = vessel.orbit.meanAnomaly;
            Orbit.meanAnomalyAtEpoch = vessel.orbit.meanAnomalyAtEpoch;
            Orbit.epoch = vessel.orbit.epoch;
            Orbit.ObT = vessel.orbit.ObT;
        }

        public VesselOrbitDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void PrintDisplay()
        {
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
                        FlightGlobals.FindVessel(VesselId)?.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);
                    }
                    break;
                case OrbitDriver.UpdateMode.UPDATE:
                    if (GUILayout.Button("Set as track phys"))
                    {
                        FlightGlobals.FindVessel(VesselId)?.orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.TRACK_Phys);
                    }
                    break;
            }

            GUILayout.EndHorizontal();

            StringBuilder.Length = 0;
            StringBuilder.AppendLine($"Semi major axis: {Orbit.semiMajorAxis}");
            StringBuilder.AppendLine($"Eccentricity: {Orbit.eccentricity}");
            StringBuilder.AppendLine($"Inclination: {Orbit.inclination}");
            StringBuilder.AppendLine($"LAN: {Orbit.LAN}");
            StringBuilder.AppendLine($"Arg Periapsis: {Orbit.argumentOfPeriapsis}");
            StringBuilder.AppendLine($"Mean anomaly: {Orbit.meanAnomaly}");
            StringBuilder.AppendLine($"Mean anomaly at Epoch: {Orbit.meanAnomalyAtEpoch}");
            StringBuilder.AppendLine($"Epoch: {Orbit.epoch}");
            StringBuilder.Append($"ObT: {Orbit.ObT}");

            GUILayout.Label(StringBuilder.ToString());
        }
    }
}
