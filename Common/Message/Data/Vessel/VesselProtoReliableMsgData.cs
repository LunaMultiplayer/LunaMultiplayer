using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselProtoReliableMsgData : VesselProtoBaseMsgData
    {
        internal VesselProtoReliableMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.ProtoReliable;

        public override string ClassName { get; } = nameof(VesselProtoReliableMsgData);
    }
}