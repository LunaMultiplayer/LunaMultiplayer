using LunaCommon.Locks;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockReleaseMsgData : LockBaseMsgData
    {
        public override LockMessageType LockMessageType => LockMessageType.Release;
        public LockDefinition Lock { get; set; }
        public bool LockResult { get; set; }
    }
}