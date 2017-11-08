using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselListReplyMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselListReplyMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.ListReply;
        public string[] Vessels { get; set; }
    }
}