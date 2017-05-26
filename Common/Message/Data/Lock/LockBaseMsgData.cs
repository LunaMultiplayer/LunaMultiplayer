using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Lock
{
    public class LockBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)LockMessageType;

        public virtual LockMessageType LockMessageType => throw new NotImplementedException();
    }
}