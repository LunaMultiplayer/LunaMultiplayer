using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal PlayerColorBaseMsgData() { }
        public override ushort SubType => (ushort)(int)PlayerColorMessageType;
        public virtual PlayerColorMessageType PlayerColorMessageType => throw new NotImplementedException();
    }
}