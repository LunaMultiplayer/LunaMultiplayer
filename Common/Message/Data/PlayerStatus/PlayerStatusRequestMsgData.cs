using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusRequestMsgData: PlayerStatusBaseMsgData
    {
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.REQUEST;
    }
}
