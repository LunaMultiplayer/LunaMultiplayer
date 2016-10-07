using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.SyncTime
{
    public class SyncTimeReplyMsgData : SyncTimeBaseMsgData
    {
        public override SyncTimeMessageType SyncTimeMessageType => SyncTimeMessageType.REPLY;
        public long ClientSendTime { get; set; }
        public long ServerReceiveTime { get; set; }
        public long ServerSendTime { get; set; }
        public long ServerStartTime { get; set; }
    }
}