using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockListRequestMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.ListRequest;
    }
}