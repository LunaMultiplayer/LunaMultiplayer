using System;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)PlayerColorMessageType;
        public virtual PlayerColorMessageType PlayerColorMessageType
        {
            get { throw new NotImplementedException(); }
        }
    }
}