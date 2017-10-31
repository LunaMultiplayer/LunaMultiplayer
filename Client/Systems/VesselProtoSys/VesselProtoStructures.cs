using System;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoUpdate
    {
        public Guid VesselId { get; set; }
        public ProtoVessel ProtoVessel { get; set; }
        public bool NeedsToBeReloaded { get; set; } = true;
        public Vessel Vessel => FlightGlobals.FindVessel(VesselId);

        public bool VesselExist => Vessel != null;

        public VesselProtoUpdate(ConfigNode vessel, Guid vesselId)
        {
            VesselId = vesselId;
            ProtoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(vessel, vesselId);
        }

        public VesselProtoUpdate(VesselProtoUpdate protoUpdate)
        {
            VesselId = protoUpdate.VesselId;
            ProtoVessel = protoUpdate.ProtoVessel;
        }
    }

    public class VesselChange
    {
        public uint[] ShieldsToClose { get; set; }
        public uint[] ShieldsToOpen { get; set; }
        public uint[] PartsToExtend { get; set; }
        public uint[] PartsToRetract { get; set; }

        public bool HasChanges()
        {
            return ShieldsToClose.Any() || ShieldsToOpen.Any() || PartsToExtend.Any() || PartsToRetract.Any();
        }
    }
}