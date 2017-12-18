using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Chat
{
    public abstract class ChatBaseMsgData : MessageData
    {
        /// <inheritdoc />
        internal ChatBaseMsgData() { }
        public override ushort SubType => (ushort)(int)ChatMessageType;
        public virtual ChatMessageType ChatMessageType => throw new NotImplementedException();

        public string From;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            lidgrenMsg.Write(From);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            From = lidgrenMsg.ReadString();
        }

        public override void Recycle()
        {
            //Nothing to implement here
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return From.GetByteCount();
        }
    }
}