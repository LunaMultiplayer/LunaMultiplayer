using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Color
{
    public class PlayerColorRequestMsgData : PlayerColorBaseMsgData
    {
        /// <inheritdoc />
        internal PlayerColorRequestMsgData() { }
        public override PlayerColorMessageType PlayerColorMessageType => PlayerColorMessageType.Request;
    }
}