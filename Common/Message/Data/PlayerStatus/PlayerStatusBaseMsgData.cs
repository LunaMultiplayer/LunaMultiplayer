using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal PlayerStatusBaseMsgData() { }
        public override ushort SubType => (ushort)(int)PlayerStatusMessageType;

        public virtual PlayerStatusMessageType PlayerStatusMessageType => throw new NotImplementedException();
    }
}
