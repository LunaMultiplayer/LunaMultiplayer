using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpSubspacesReplyMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpSubspacesReplyMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.SubspacesReply;

        public int SubspaceCount;
        public SubspaceInfo[] Subspaces = new SubspaceInfo[0];

        public override string ClassName { get; } = nameof(WarpSubspacesReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(SubspaceCount);
            for (var i = 0; i < SubspaceCount; i++)
            {
                Subspaces[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            SubspaceCount = lidgrenMsg.ReadInt32();
            Subspaces = ArrayPool<SubspaceInfo>.Claim(SubspaceCount);
            for (var i = 0; i < SubspaceCount; i++)
            {
                if (Subspaces[i] == null)
                    Subspaces[i] = new SubspaceInfo();

                Subspaces[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            for (var i = 0; i < SubspaceCount; i++)
            {
                Subspaces[i].Recycle();
            }
            ArrayPool<SubspaceInfo>.Release(ref Subspaces);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < SubspaceCount; i++)
            {
                arraySize += Subspaces[i].GetByteCount(dataCompressed);
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}