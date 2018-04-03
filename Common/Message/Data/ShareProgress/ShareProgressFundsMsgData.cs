using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending funds changes between clients.
    /// </summary>
    public class ShareProgressFundsMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressFundsMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.FundsUpdate;

        public double Funds;
        public string Reason;

        public override string ClassName { get; } = nameof(ShareProgressFundsMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Funds);
            lidgrenMsg.Write(Reason);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Funds = lidgrenMsg.ReadDouble();
            Reason = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(double);
        }
    }
}
