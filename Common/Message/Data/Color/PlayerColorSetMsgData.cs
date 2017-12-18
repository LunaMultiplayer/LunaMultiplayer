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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            PlayerColor.Serialize(lidgrenMsg, dataCompressed);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerColor.Deserialize(lidgrenMsg, dataCompressed);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + PlayerColor.GetByteCount();
        }
    }
}