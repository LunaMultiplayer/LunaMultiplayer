using LmpClient.Extensions;
using System;
using UnityEngine;

namespace LmpClient.Windows.Vessels.Structures
{

    internal class VesselDataDisplay : VesselBaseDisplay
    {
        public override bool Display { get; set; }
        public Guid VesselId { get; set; }
        public bool Loaded { get; set; }
        public bool Packed { get; set; }
        public bool Immortal { get; set; }

        public VesselDataDisplay(Guid vesselId) => VesselId = vesselId;

        protected override void UpdateDisplay(Vessel vessel)
        {
            VesselId = vessel.id;
            Loaded = vessel.loaded;
            Packed = vessel.packed;
            Immortal = vessel.IsImmortal();
        }

        protected override void PrintDisplay()
        {
            StringBuilder.Length = 0;
            StringBuilder.Append("Loaded: ").AppendLine(Loaded.ToString())
                .Append("Packed: ").AppendLine(Packed.ToString())
                .Append("Immortal: ").Append(Immortal);

            GUILayout.Label(StringBuilder.ToString());
        }
    }
}
