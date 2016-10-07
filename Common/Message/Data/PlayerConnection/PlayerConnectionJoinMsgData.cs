using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionJoinMsgData : PlayerConnectionBaseMsgData
    {
        public override PlayerConnectionMessageType PlayerConnectionMessageType => PlayerConnectionMessageType.JOIN;
    }
}