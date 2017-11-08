using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal FlagBaseMsgData() { }
        public override ushort SubType => (ushort)(int)FlagMessageType;
        public virtual FlagMessageType FlagMessageType => throw new NotImplementedException();
    }
}