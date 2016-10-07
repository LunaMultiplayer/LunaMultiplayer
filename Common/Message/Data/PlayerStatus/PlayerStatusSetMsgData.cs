using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusSetMsgData: PlayerStatusBaseMsgData
    {
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.SET;

        public string PlayerName { get; set; }
        public string VesselText { get; set; }
        public string StatusText { get; set; }
    }
}
