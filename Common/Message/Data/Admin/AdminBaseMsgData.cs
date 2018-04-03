using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Admin
{
    public abstract class AdminBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal AdminBaseMsgData() { }
        public override ushort SubType => (ushort)(int)AdminMessageType;
        public virtual AdminMessageType AdminMessageType => throw new NotImplementedException();
        
        public string AdminPassword;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(AdminPassword);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            AdminPassword = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return AdminPassword.GetByteCount();
        }
    }
}
