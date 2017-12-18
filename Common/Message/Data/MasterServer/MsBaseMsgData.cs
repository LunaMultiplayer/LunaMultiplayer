using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.MasterServer
{
    public abstract class MsBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal MsBaseMsgData() { }

        // Master server messages does not have versions!

        public override ushort MajorVersion => 0;
        public override ushort MinorVersion => 0;
        public override ushort BuildVersion => 0;

        public override ushort SubType => (ushort)(int)MasterServerMessageSubType;
        public virtual MasterServerMessageSubType MasterServerMessageSubType => throw new NotImplementedException();

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            //Nothing to implement here
        }

        public override void Recycle()
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return 0;
        }
    }
}
