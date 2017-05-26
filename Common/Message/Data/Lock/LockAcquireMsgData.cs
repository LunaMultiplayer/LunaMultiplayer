using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockAcquireMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.Acquire;
        public string PlayerName { get; set; }
        public string LockName { get; set; }
        public bool LockResult { get; set; }
        public bool Force { get; set; }
    }
}