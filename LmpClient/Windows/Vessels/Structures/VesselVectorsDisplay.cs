using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselVectorsDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public Vessel Vessel { get; set; }

        public VesselVectorsDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            Vessel = vessel;
        }

        protected override void PrintDisplay()
        {
            if (!Vessel) return;

            StringBuilder.Length = 0;
            StringBuilder.AppendLine($"Forward vector: {Vessel.GetFwdVector()}");
            StringBuilder.AppendLine($"Up vector: {Vessel.upAxis}");
            StringBuilder.AppendLine($"Srf Rotation: {Vessel.srfRelRotation}");
            StringBuilder.AppendLine($"Vessel Rotation: {Vessel.transform.rotation}");
            StringBuilder.AppendLine($"Vessel Local Rotation: {Vessel.transform.localRotation}");
            StringBuilder.AppendLine($"mainBody Rotation: {Vessel.mainBody.rotation}");
            StringBuilder.AppendLine($"mainBody Transform Rotation: {Vessel.mainBody.bodyTransform.rotation}");
            StringBuilder.AppendLine($"Surface Velocity: {Vessel.GetSrfVelocity()}, |v|: {Vessel.GetSrfVelocity().magnitude}");
            StringBuilder.AppendLine($"Orbital Velocity: {Vessel.GetObtVelocity()}, |v|: {Vessel.GetObtVelocity().magnitude}");
            if (Vessel.orbitDriver != null && Vessel.orbitDriver.orbit != null)
                StringBuilder.AppendLine($"Frame Velocity: {Vessel.orbitDriver.orbit.GetFrameVel()}, |v|: {Vessel.orbitDriver.orbit.GetFrameVel().magnitude}");
            StringBuilder.AppendLine($"CoM offset vector: {Vessel.CoM}");
            StringBuilder.Append($"Angular Velocity: {Vessel.angularVelocity}, |v|: {Vessel.angularVelocity.magnitude}");

            GUILayout.Label(StringBuilder.ToString());
        }
    }
}
