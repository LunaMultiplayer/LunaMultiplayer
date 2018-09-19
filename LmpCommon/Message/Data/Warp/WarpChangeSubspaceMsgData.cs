using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Warp
{
    public class WarpChangeSubspaceMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpChangeSubspaceMsgData(){}
        public override WarpMessageType WarpMessageType => WarpMessageType.ChangeSubspace;

        public string PlayerName;
        public int Subspace;

        public override string ClassName { get; } = nameof(WarpChangeSubspaceMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.Write(Subspace);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PlayerName = lidgrenMsg.ReadString();
            Subspace = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerName.GetByteCount() + sizeof(int);
        }
    }
}