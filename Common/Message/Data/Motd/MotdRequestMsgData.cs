using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Motd
{
    public class MotdRequestMsgData : MotdBaseMsgData
    {
        /// <inheritdoc />
        internal MotdRequestMsgData() { }
        public override MotdMessageType MotdMessageType => MotdMessageType.Request;

        public override string ClassName { get; } = nameof(MotdRequestMsgData);
    }
}