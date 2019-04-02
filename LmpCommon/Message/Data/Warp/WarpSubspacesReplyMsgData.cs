using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Warp
{
    public class WarpSubspacesReplyMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpSubspacesReplyMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.SubspacesReply;

        public int SubspaceCount;
        public SubspaceInfo[] Subspaces = new SubspaceInfo[0];

        public override string ClassName { get; } = nameof(WarpSubspacesReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(SubspaceCount);
            for (var i = 0; i < SubspaceCount; i++)
            {
                Subspaces[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            SubspaceCount = lidgrenMsg.ReadInt32();
            if (Subspaces.Length < SubspaceCount)
                Subspaces = new SubspaceInfo[SubspaceCount];

            for (var i = 0; i < SubspaceCount; i++)
            {
                if (Subspaces[i] == null)
                    Subspaces[i] = new SubspaceInfo();

                Subspaces[i].Deserialize(lidgrenMsg);
            }
        }

        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < SubspaceCount; i++)
            {
                arraySize += Subspaces[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}