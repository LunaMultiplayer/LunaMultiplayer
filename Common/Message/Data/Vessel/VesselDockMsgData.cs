using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselDockMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.Dock;
        public int Subspace { get; set; }
        public Guid DominantVesselId { get; set; }
        public Guid WeakVesselId { get; set; }
        public byte[] FinalVesselData { get; set; }
    }
}