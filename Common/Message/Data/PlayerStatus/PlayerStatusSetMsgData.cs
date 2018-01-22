using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusSetMsgData: PlayerStatusBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerStatusSetMsgData() { }
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.Set;

        public PlayerStatusInfo PlayerStatus = new PlayerStatusInfo();

        public override string ClassName { get; } = nameof(PlayerStatusSetMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            PlayerStatus.Serialize(lidgrenMsg, compressData);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerStatus.Deserialize(lidgrenMsg, dataCompressed);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + PlayerStatus.GetByteCount(dataCompressed);
        }
    }
}
