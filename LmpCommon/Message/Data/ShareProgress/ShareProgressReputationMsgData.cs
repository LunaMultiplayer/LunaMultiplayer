using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending reputation changes between clients.
    /// </summary>
    public class ShareProgressReputationMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressReputationMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.ReputationUpdate;

        public float Reputation;
        public string Reason;

        public override string ClassName { get; } = nameof(ShareProgressReputationMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Reputation);
            lidgrenMsg.Write(Reason);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            Reputation = lidgrenMsg.ReadFloat();
            Reason = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(float);
        }
    }
}
