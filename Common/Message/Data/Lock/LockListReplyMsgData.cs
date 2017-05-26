using System.Collections.Generic;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockListReplyMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.ListReply;
        public KeyValuePair<string, string>[] Locks { get; set; }
    }
}