using LmpClient.VesselUtilities;
using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public string VesselName { get; set; }
        public VesselDataDisplay Data { get; set; }
        public VesselLockDisplay Locks { get; set; }
        public VesselOrbitDisplay Orbit { get; set; }
        public VesselInterpolationDisplay Interpolation { get; set; }

        public VesselDisplay(Guid vesselId)
        {
            VesselId = vesselId;
            Data = new VesselDataDisplay(VesselId);
            Locks = new VesselLockDisplay(VesselId);
            Orbit = new VesselOrbitDisplay(VesselId);
            Interpolation = new VesselInterpolationDisplay(VesselId);
        }

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            VesselName = vessel.vesselName;
            Data.Update(vessel);
            Locks.Update(vessel);
            Orbit.Update(vessel);
            Interpolation.Update(vessel);
        }

        protected override void PrintDisplay()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(VesselName);
            if (GUILayout.Button("Reload"))
            {
                var vessel = FlightGlobals.FindVessel(VesselId);
                vessel.protoVessel = vessel.BackupVessel();
                VesselLoader.LoadVessel(vessel.protoVessel);
            }
            Data.Display = GUILayout.Toggle(Data.Display, nameof(Data), ButtonStyle);
            Data.Print();
            Locks.Display = GUILayout.Toggle(Locks.Display, nameof(Locks), ButtonStyle);
            Locks.Print();
            Orbit.Display = GUILayout.Toggle(Orbit.Display, nameof(Orbit), ButtonStyle);
            Orbit.Print();
            Interpolation.Display = GUILayout.Toggle(Interpolation.Display, nameof(Interpolation), ButtonStyle);
            Interpolation.Print();
            GUILayout.EndVertical();
        }
    }
}
