using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Warp
{
    public class WarpNewSubspaceMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpNewSubspaceMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.NewSubspace;

        public string PlayerCreator;
        public int SubspaceKey;
        public double ServerTimeDifference;

        public override string ClassName { get; } = nameof(WarpNewSubspaceMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PlayerCreator);
            lidgrenMsg.Write(SubspaceKey);
            lidgrenMsg.Write(ServerTimeDifference);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PlayerCreator = lidgrenMsg.ReadString();
            SubspaceKey = lidgrenMsg.ReadInt32();
            ServerTimeDifference = lidgrenMsg.ReadDouble();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerCreator.GetByteCount() + sizeof(int) + sizeof(double);
        }
    }
}