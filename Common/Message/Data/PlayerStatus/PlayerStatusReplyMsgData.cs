using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusReplyMsgData: PlayerStatusBaseMsgData
    {
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.Reply;

        public string[] PlayerName { get; set; }
        public string[] VesselText { get; set; }
        public string[] StatusText { get; set; }
    }
}
