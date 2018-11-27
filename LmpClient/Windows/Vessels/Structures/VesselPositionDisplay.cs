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
            StringBuilder.AppendLine($"Situation: {Vessel.situation}");
            StringBuilder.AppendLine($"Orbit Pos: {Vessel.orbit.pos}");
            StringBuilder.AppendLine($"Transform Pos: {Vessel.vesselTransform.position}");
            StringBuilder.AppendLine($"Com Pos: {Vessel.CoM}");
            StringBuilder.AppendLine($"ComD Pos: {Vessel.CoMD}");
            StringBuilder.AppendLine($"Lat,Lon,Alt: {Vessel.latitude},{Vessel.longitude},{Vessel.altitude}");

            Vessel.mainBody.GetLatLonAlt(Vessel.vesselTransform.position, out var lat, out var lon, out var alt);
            StringBuilder.AppendLine($"Current Lat,Lon,Alt: {lat},{lon},{alt}");

            Vessel.mainBody.GetLatLonAltOrbital(Vessel.orbit.pos, out lat, out lon, out alt);
            StringBuilder.AppendLine($"Orbital Lat,Lon,Alt: {lat},{lon},{alt}");

            StringBuilder.AppendLine($"Inside safety bubble: {SafetyBubbleSystem.Singleton.IsInSafetyBubble(Vessel)}");

            GUILayout.Label(StringBuilder.ToString());
        }
    }
}
