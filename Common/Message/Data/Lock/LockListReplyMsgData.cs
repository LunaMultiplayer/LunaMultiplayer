using LunaCommon.Locks;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockListReplyMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.ListReply;
        public LockDefinition[] Locks { get; set; }
    }
}