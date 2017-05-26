using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.SyncTime
{
    public class SyncTimeRequestMsgData : SyncTimeBaseMsgData
    {
        public override SyncTimeMessageType SyncTimeMessageType => SyncTimeMessageType.Request;
        public long ClientSendTime { get; set; }
    }
}