using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselOrbitDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }

        public OrbitDriver.UpdateMode ObtDriverMode { get; set; }

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            ObtDriverMode = vessel.orbitDriver.updateMode;
        }

        public VesselOrbitDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void PrintDisplay()
        {
            GUILayout.BeginHorizontal();

            StringBuilder.Length = 0;
            StringBuilder.Append("Update mode: ").AppendLine(ObtDriverMode.ToString());

            GUILayout.Label(StringBuilder.ToString());

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
        }
    }
}
