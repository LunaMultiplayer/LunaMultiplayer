using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionLeaveMsgData : PlayerConnectionBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerConnectionLeaveMsgData() { }
        public override PlayerConnectionMessageType PlayerConnectionMessageType => PlayerConnectionMessageType.Leave;
    }
}