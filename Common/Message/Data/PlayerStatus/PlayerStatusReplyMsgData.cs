using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusReplyMsgData: PlayerStatusBaseMsgData
    {
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.REPLY;

        public string[] PlayerName { get; set; }
        public string[] VesselText { get; set; }
        public string[] StatusText { get; set; }
    }
}
