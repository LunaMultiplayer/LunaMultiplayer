using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Flag
{
    public abstract class FlagBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal FlagBaseMsgData() { }
        public override ushort SubType => (ushort)(int)FlagMessageType;
        public virtual FlagMessageType FlagMessageType => throw new NotImplementedException();

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