using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionJoinMsgData : PlayerConnectionBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerConnectionJoinMsgData() { }
        public override PlayerConnectionMessageType PlayerConnectionMessageType => PlayerConnectionMessageType.Join;
    }
}