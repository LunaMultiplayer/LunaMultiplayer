using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending experimental parts
    /// </summary>
    public class ShareProgressExperimentalPartMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressExperimentalPartMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.ExperimentalPart;

        public string PartName;
        public int Count;

        public override string ClassName { get; } = nameof(ShareProgressExperimentalPartMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PartName);
            lidgrenMsg.Write(Count);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PartName = lidgrenMsg.ReadString();
            Count = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PartName.GetByteCount() + sizeof(int);
        }
    }
}
