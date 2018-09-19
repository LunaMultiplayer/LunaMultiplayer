using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending part purchases between clients.
    /// </summary>
    public class ShareProgressPartPurchaseMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressPartPurchaseMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.PartPurchase;

        public string TechId;
        public string PartName;

        public override string ClassName { get; } = nameof(ShareProgressPartPurchaseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            lidgrenMsg.Write(TechId);
            lidgrenMsg.Write(PartName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            TechId = lidgrenMsg.ReadString();
            PartName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + TechId.GetByteCount() + PartName.GetByteCount();
        }
    }
}
