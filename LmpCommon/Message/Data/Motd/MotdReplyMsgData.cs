using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Motd
{
    public class MotdReplyMsgData : MotdBaseMsgData
    {
        /// <inheritdoc />
        internal MotdReplyMsgData() { }
        public override MotdMessageType MotdMessageType => MotdMessageType.Reply;

        public string MessageOfTheDay;

        public override string ClassName { get; } = nameof(MotdReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(MessageOfTheDay);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            MessageOfTheDay = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + MessageOfTheDay.GetByteCount();
        }
    }
}