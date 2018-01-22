using Lidgren.Network;
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

        public override string ClassName { get; } = nameof(PlayerColorReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool compressData)
        {
            base.InternalSerialize(lidgrenMsg, compressData);

            lidgrenMsg.Write(PlayerColorsCount);

            for (var i = 0; i < PlayerColorsCount; i++)
            {
                PlayersColors[i].Serialize(lidgrenMsg, compressData);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerColorsCount = lidgrenMsg.ReadInt32();
            
            if (PlayersColors.Length < PlayerColorsCount)
                PlayersColors = new PlayerColor[PlayerColorsCount];

            for (var i = 0; i < PlayerColorsCount; i++)
            {
                if (PlayersColors[i] == null)
                    PlayersColors[i] = new PlayerColor();

                PlayersColors[i].Deserialize(lidgrenMsg, dataCompressed);
            }
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