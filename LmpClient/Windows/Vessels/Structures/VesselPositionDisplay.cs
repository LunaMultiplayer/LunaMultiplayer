using LmpClient.Systems.SafetyBubble;
using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{
    internal class VesselPositionDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public Vessel Vessel { get; set; }

        public VesselPositionDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            Vessel = vessel;
        }

        protected override void PrintDisplay()
        {
            if (!Vessel) return;

            StringBuilder.Length = 0;
            StringBuilder.AppendLine($"Situation: {Vessel.situation}")
                .AppendLine($"Orbit Pos: {Vessel.orbit.pos}")
                .AppendLine($"Transform Pos: {Vessel.vesselTransform.position}")
                .AppendLine($"Com Pos: {Vessel.CoM}")
                .AppendLine($"ComD Pos: {Vessel.CoMD}")
                .AppendLine($"Lat,Lon,Alt: {Vessel.latitude},{Vessel.longitude},{Vessel.altitude}");

            Vessel.mainBody.GetLatLonAlt(Vessel.vesselTransform.position, out var lat, out var lon, out var alt);
            StringBuilder.AppendLine($"Current Lat,Lon,Alt: {lat},{lon},{alt}");

            Vessel.mainBody.GetLatLonAltOrbital(Vessel.orbit.pos, out lat, out lon, out alt);
            StringBuilder.AppendLine($"Orbital Lat,Lon,Alt: {lat},{lon},{alt}");

            StringBuilder.Append($"Inside safety bubble: {SafetyBubbleSystem.Singleton.IsInSafetyBubble(Vessel)}");

            GUILayout.Label(StringBuilder.ToString());
        }
    }
}
