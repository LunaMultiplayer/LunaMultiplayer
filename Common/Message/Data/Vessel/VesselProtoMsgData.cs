using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselProtoMsgData : VesselProtoBaseMsgData
    {
        internal VesselProtoMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Proto;

        public override string ClassName { get; } = nameof(VesselProtoMsgData);
    }
}