using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselListRequestMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselListRequestMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.ListRequest;
    }
}