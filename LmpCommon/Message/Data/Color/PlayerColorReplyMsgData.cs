using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Color
{
    public class PlayerColorReplyMsgData : PlayerColorBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerColorReplyMsgData() { }
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.Reply;

        public int PlayerColorsCount;
        public PlayerColor[] PlayersColors = new PlayerColor[0];

        public override string ClassName { get; } = nameof(PlayerColorReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(PlayerColorsCount);

            for (var i = 0; i < PlayerColorsCount; i++)
            {
                PlayersColors[i].Serialize(lidgrenMsg);
            }
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PlayerColorsCount = lidgrenMsg.ReadInt32();
            
            if (PlayersColors.Length < PlayerColorsCount)
                PlayersColors = new PlayerColor[PlayerColorsCount];

            for (var i = 0; i < PlayerColorsCount; i++)
            {
                if (PlayersColors[i] == null)
                    PlayersColors[i] = new PlayerColor();

                PlayersColors[i].Deserialize(lidgrenMsg);
            }
        }
        
        internal override int InternalGetMessageSize()
        {
            var arraySize = 0;
            for (var i = 0; i < PlayerColorsCount; i++)
            {
                arraySize += PlayersColors[i].GetByteCount();
            }

            return base.InternalGetMessageSize() + sizeof(int) + arraySize;
        }
    }
}