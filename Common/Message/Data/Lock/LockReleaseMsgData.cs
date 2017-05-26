using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockReleaseMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.Release;
        public string PlayerName { get; set; }
        public string LockName { get; set; }
        public bool LockResult { get; set; }
    }
}