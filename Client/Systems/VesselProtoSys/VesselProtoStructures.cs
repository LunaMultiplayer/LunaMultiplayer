using System;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoUpdate
    {
        public Guid VesselId { get; set; }
        public ProtoVessel ProtoVessel { get; set; }
        public bool Loaded { get; set; }

        public VesselProtoUpdate(ConfigNode vessel, Guid vesselId)
        {
            VesselId = vesselId;
            ProtoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(vessel, vesselId);
        }
    }
}