using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.SyncTime
{
    public class SyncTimeReplyMsgData : SyncTimeBaseMsgData
    {
        /// <inheritdoc />
        internal SyncTimeReplyMsgData() { }
        public override SyncTimeMessageType SyncTimeMessageType => SyncTimeMessageType.Reply;
        public long ClientSendTime { get; set; }
        public long ServerReceiveTime { get; set; }
        public long ServerSendTime { get; set; }
        public long ServerStartTime { get; set; }
    }
}