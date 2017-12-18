using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselsRequestMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselsRequestMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.VesselsRequest;

        public override string ClassName { get; } = nameof(VesselsRequestMsgData);

    }
}