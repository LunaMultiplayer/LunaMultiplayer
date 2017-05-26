using System;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoUpdate
    {
        public Guid VesselId { get; set; }
        public ConfigNode VesselNode { get; set; }
        public bool Loaded { get; set; }
    }
}