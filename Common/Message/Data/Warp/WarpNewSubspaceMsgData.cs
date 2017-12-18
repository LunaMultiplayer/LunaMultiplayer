using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpNewSubspaceMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpNewSubspaceMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.NewSubspace;

        public string PlayerCreator;
        public int SubspaceKey;
        public double ServerTimeDifference;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(PlayerCreator);
            lidgrenMsg.Write(SubspaceKey);
            lidgrenMsg.Write(ServerTimeDifference);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerCreator = lidgrenMsg.ReadString();
            SubspaceKey = lidgrenMsg.ReadInt32();
            ServerTimeDifference = lidgrenMsg.ReadDouble();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + PlayerCreator.GetByteCount() + sizeof(int) + sizeof(double);
        }
    }
}