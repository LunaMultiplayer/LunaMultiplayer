using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusSetMsgData: PlayerStatusBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerStatusSetMsgData() { }
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.Set;

        public PlayerStatusInfo PlayerStatus = new PlayerStatusInfo();

        public override string ClassName { get; } = nameof(PlayerStatusSetMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            PlayerStatus.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PlayerStatus.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerStatus.GetByteCount();
        }
    }
}
