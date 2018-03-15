using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Data.ShareProgress
{
    public abstract class ShareProgressBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal ShareProgressBaseMsgData() { }
        public override ushort SubType => (ushort)(int)ShareProgressMessageType;
        public virtual ShareProgressMessageType ShareProgressMessageType => throw new NotImplementedException();

        public string ObjectId;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(ObjectId);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            ObjectId = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return ObjectId.GetByteCount();
        }
    }
}
