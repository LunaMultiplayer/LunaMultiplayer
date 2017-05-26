using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorSetMsgData : PlayerColorBaseMsgData
    {
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.Set;
        public string PlayerName { get; set; }
        public string Color { get; set; }
    }
}