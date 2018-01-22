using Lidgren.Network;
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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            lidgrenMsg.Write(SubspaceCount);
            for (var i = 0; i < SubspaceCount; i++)
            {
                Subspaces[i].Serialize(lidgrenMsg, compressData);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            SubspaceCount = lidgrenMsg.ReadInt32();
            if (Subspaces.Length < SubspaceCount)
                Subspaces = new SubspaceInfo[SubspaceCount];

            for (var i = 0; i < SubspaceCount; i++)
            {
                if (Subspaces[i] == null)
                    Subspaces[i] = new SubspaceInfo();

                Subspaces[i].Deserialize(lidgrenMsg, dataCompressed);
            }
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