using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Kerbal
{
    public abstract class KerbalBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal KerbalBaseMsgData() { }
        public override ushort SubType => (ushort)(int)KerbalMessageType;

        public virtual KerbalMessageType KerbalMessageType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }
        
        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return 0;
        }
    }
}
