using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Motd
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