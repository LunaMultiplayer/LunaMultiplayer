using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;
using System;

namespace LmpCommon.Message.Data.Kerbal
{
    public abstract class KerbalBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal KerbalBaseMsgData() { }
        public override ushort SubType => (ushort)(int)KerbalMessageType;

        public virtual KerbalMessageType KerbalMessageType => throw new NotImplementedException();

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
