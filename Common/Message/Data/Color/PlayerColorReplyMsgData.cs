using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorReplyMsgData : PlayerColorBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerColorReplyMsgData() { }
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.Reply;

        public int PlayerColorsCount;
        public PlayerColor[] PlayersColors = new PlayerColor[0];

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(PlayerColorsCount);

            for (var i = 0; i < PlayerColorsCount; i++)
            {
                PlayersColors[i].Serialize(lidgrenMsg, dataCompressed);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerColorsCount = lidgrenMsg.ReadInt32();

            PlayersColors = ArrayPool<PlayerColor>.Claim(PlayerColorsCount);
            for (var i = 0; i < PlayerColorsCount; i++)
            {
                if (PlayersColors[i] == null)
                    PlayersColors[i] = new PlayerColor();

                PlayersColors[i].Deserialize(lidgrenMsg, dataCompressed);
            }
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<PlayerColor>.Release(ref PlayersColors);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            var arraySize = 0;
            for (var i = 0; i < PlayerColorsCount; i++)
            {
                arraySize += PlayersColors[i].GetByteCount();
            }

            return base.InternalGetMessageSize(dataCompressed) + sizeof(int) + arraySize;
        }
    }
}