using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.PlayerStatus
{
    public class PlayerStatusRequestMsgData : PlayerStatusBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerStatusRequestMsgData() { }
        public override PlayerStatusMessageType PlayerStatusMessageType => PlayerStatusMessageType.Request;

        public override string ClassName { get; } = nameof(PlayerStatusRequestMsgData);
    }
}
