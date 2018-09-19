using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending science experiments between clients.
    /// </summary>
    public class ShareProgressScienceSubjectMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressScienceSubjectMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.ScienceSubjectUpdate;

        public ScienceSubjectInfo ScienceSubject = new ScienceSubjectInfo();

        public override string ClassName { get; } = nameof(ShareProgressScienceSubjectMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            ScienceSubject.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            ScienceSubject.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + ScienceSubject.GetByteCount();
        }
    }
}
