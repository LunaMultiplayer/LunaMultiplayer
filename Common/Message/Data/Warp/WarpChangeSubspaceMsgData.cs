using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpChangeSubspaceMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpChangeSubspaceMsgData(){}
        public override WarpMessageType WarpMessageType => WarpMessageType.ChangeSubspace;

        public string PlayerName;
        public int Subspace;

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.Write(Subspace);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerName = lidgrenMsg.ReadString();
            Subspace = lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + PlayerName.GetByteCount() + sizeof(int);
        }
    }
}