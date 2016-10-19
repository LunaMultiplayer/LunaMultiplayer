using System;
using LunaClient.Systems.VesselUpdateSys;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoUpdate
    {
        public Guid VesselId { get; set; }
        public ConfigNode VesselNode { get; set; }
        public bool Loaded { get; set; }
        public bool HasUpdates => VesselUpdateSystem.Singleton.VesselHasUpdates(VesselId, 1);
    }
}