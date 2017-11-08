using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal AdminBaseMsgData() { }

        public override ushort SubType => (ushort)(int)AdminMessageType;

        public virtual AdminMessageType AdminMessageType => throw new NotImplementedException();
    }
}