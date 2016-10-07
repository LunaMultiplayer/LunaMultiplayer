using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorSetMsgData : PlayerColorBaseMsgData
    {
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.SET;
        public string PlayerName { get; set; }
        public string Color { get; set; }
    }
}