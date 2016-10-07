using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockReleaseMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.RELEASE;
        public string PlayerName { get; set; }
        public string LockName { get; set; }
        public bool LockResult { get; set; }
    }
}