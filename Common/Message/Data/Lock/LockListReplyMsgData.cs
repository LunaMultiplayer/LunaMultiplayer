using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockListReplyMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.LIST_REPLY;
        public KeyValuePair<string, string>[] Locks { get; set; }
    }
}