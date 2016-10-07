using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorRequestMsgData : PlayerColorBaseMsgData
    {
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.REQUEST;
    }
}