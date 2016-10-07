using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionLeaveMsgData : PlayerConnectionBaseMsgData
    {
        public override PlayerConnectionMessageType PlayerConnectionMessageType => PlayerConnectionMessageType.LEAVE;
    }
}