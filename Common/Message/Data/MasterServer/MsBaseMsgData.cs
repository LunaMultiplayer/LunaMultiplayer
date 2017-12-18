using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal MsBaseMsgData() { }

        /// <inheritdoc />
        /// <summary>
        /// Master server messages does not have versions!
        /// </summary>
        public override string Version => "0.0.0.0";
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
