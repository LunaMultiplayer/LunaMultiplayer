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

        public override string ClassName { get; } = nameof(WarpChangeSubspaceMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

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