using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorSetMsgData : PlayerColorBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerColorSetMsgData(){}
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.Set;

        public PlayerColor PlayerColor = new PlayerColor();

        public override string ClassName { get; } = nameof(PlayerColorSetMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            PlayerColor.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PlayerColor.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerColor.GetByteCount();
        }
    }
}