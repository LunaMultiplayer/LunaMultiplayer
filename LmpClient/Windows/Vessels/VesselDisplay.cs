using System;

namespace LmpClient.Windows.Vessels
{
    internal class VesselDisplay
    {
        public Guid VesselId { get; set; }
        public string VesselName { get; set; }
        public bool Selected { get; set; }
        public bool Loaded { get; set; }
        public bool Packed { get; set; }
        public bool Immortal { get; set; }
        public OrbitDriver.UpdateMode ObtDriverMode { get; set; }
    }
}
