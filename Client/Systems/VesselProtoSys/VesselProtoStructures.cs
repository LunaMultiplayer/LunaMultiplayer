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
        public int Stage { get; set; } = int.MinValue;
        public uint[] PartsToRemove { get; set; }
        public uint[] ShieldsToClose { get; set; }
        public uint[] ShieldsToOpen { get; set; }
        public uint[] PartsToExtend { get; set; }
        public uint[] PartsToRetract { get; set; }

        public bool HasChanges()
        {
            return Stage > int.MinValue ||
                (PartsToRemove != null && PartsToRemove.Any()) ||
                (ShieldsToClose != null && ShieldsToClose.Any()) || 
                (ShieldsToOpen != null && ShieldsToOpen.Any()) || 
                (PartsToExtend != null && PartsToExtend.Any()) || 
                (PartsToRetract != null && PartsToRetract.Any());
        }
    }
}