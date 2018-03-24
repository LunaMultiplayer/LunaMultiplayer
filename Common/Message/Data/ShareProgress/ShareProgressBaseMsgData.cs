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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize()
        {
            return 0;
        }
    }
}
