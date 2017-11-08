using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal GroupBaseMsgData() { }
        public override ushort SubType => (ushort)(int)GroupMessageType;

        public virtual GroupMessageType GroupMessageType => throw new NotImplementedException();
    }
}