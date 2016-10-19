using System;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselChangeMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.CHANGE;
        public Guid VesselId { get; set; }
        public uint PartFlightId { get; set; }
        public uint PartCraftId { get; set; }
        public int ChangeType { get; set; }
        public int Stage { get; set; }
    }
}
