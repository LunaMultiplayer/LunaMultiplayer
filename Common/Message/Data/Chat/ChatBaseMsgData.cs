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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(From);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            From = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return From.GetByteCount();
        }
    }
}