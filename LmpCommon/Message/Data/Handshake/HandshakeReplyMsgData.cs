using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Handshake
{
    public class HandshakeReplyMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeReplyMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Reply;

        public HandshakeReply Response;
        public string Reason;
        public bool ModControl;
        public long ServerStartTime;
        public string ModFileData;

        public override string ClassName { get; } = nameof(HandshakeReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write((int)Response);
            lidgrenMsg.Write(Reason);

            lidgrenMsg.Write(ModControl);
            lidgrenMsg.WritePadBits();

            lidgrenMsg.Write(ServerStartTime);
            lidgrenMsg.Write(ModFileData);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Response = (HandshakeReply)lidgrenMsg.ReadInt32();
            Reason = lidgrenMsg.ReadString();

            ModControl = lidgrenMsg.ReadBoolean();
            lidgrenMsg.SkipPadBits();

            ServerStartTime = lidgrenMsg.ReadInt64();

            ModFileData = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(HandshakeReply) + Reason.GetByteCount() + sizeof(byte) //We write pad bits so it's size of byte
                + sizeof(long) + ModFileData.GetByteCount();
        }
    }
}
