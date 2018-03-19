using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public abstract class CraftLibraryBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal CraftLibraryBaseMsgData() { }
        public override ushort SubType => (ushort)(int)CraftMessageType;
        public virtual CraftMessageType CraftMessageType => throw new NotImplementedException();

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
