using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionLeaveMsgData : PlayerConnectionBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerConnectionLeaveMsgData() { }
        public override PlayerConnectionMessageType PlayerConnectionMessageType => PlayerConnectionMessageType.Leave;

        public override string ClassName { get; } = nameof(PlayerConnectionLeaveMsgData);
    }
}