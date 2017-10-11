using System;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoUpdate
    {
        public Guid VesselId { get; set; }
        public ProtoVessel ProtoVessel { get; set; }
        public bool Loaded { get; set; }

        private Vessel _vessel;
        public Vessel Vessel
        {
            get
            {
                if (_vessel == null && VesselExist)
                    _vessel = FlightGlobals.Vessels.First(v => v.id == VesselId);

                return _vessel;
            }
        }

        private bool? _vesselExist;
        public bool VesselExist
        {
            get
            {
                if (!_vesselExist.HasValue)
                    _vesselExist = FlightGlobals.Vessels.Any(v => v.id == VesselId);

                return _vesselExist.Value;
            }
        }

        public VesselProtoUpdate(ConfigNode vessel, Guid vesselId)
        {
            VesselId = vesselId;
            ProtoVessel = VesselCommon.CreateSafeProtoVesselFromConfigNode(vessel, vesselId);
        }
    }
}