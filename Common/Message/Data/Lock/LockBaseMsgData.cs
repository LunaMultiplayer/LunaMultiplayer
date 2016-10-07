using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Lock
{
    public class LockBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)LockMessageType;

        public virtual LockMessageType LockMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}