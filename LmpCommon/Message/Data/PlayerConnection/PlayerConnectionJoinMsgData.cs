using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.PlayerConnection
{
    public class PlayerConnectionJoinMsgData : PlayerConnectionBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerConnectionJoinMsgData() { }
        public override PlayerConnectionMessageType PlayerConnectionMessageType => PlayerConnectionMessageType.Join;

        public override string ClassName { get; } = nameof(PlayerConnectionJoinMsgData);
    }
}