using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Sent by a client when it reverts to launch/editor so the server can restore
    /// the science-subject state to the snapshot taken before that flight.
    /// Each entry uses IsDelta=false so the server can reconstruct the full node.
    /// </summary>
    public class ShareProgressScienceSubjectRevertMsgData : ShareProgressBaseMsgData
    {
        internal ShareProgressScienceSubjectRevertMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.ScienceSubjectRevert;
        public override string ClassName { get; } = nameof(ShareProgressScienceSubjectRevertMsgData);

        public int SubjectCount;
        public ScienceSubjectInfo[] Subjects = new ScienceSubjectInfo[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            lidgrenMsg.Write(SubjectCount);
            for (var i = 0; i < SubjectCount; i++)
                Subjects[i].Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            SubjectCount = lidgrenMsg.ReadInt32();
            if (Subjects.Length < SubjectCount)
                Subjects = new ScienceSubjectInfo[SubjectCount];
            for (var i = 0; i < SubjectCount; i++)
            {
                if (Subjects[i] == null) Subjects[i] = new ScienceSubjectInfo();
                Subjects[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var size = base.InternalGetMessageSize() + sizeof(int);
            for (var i = 0; i < SubjectCount; i++)
                size += Subjects[i].GetByteCount();
            return size;
        }
    }
}
