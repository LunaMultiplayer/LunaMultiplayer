using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Motd
{
    public class MotdRequestMsgData : MotdBaseMsgData
    {
        /// <inheritdoc />
        internal MotdRequestMsgData() { }
        public override MotdMessageType MotdMessageType => MotdMessageType.Request;

        public override string ClassName { get; } = nameof(MotdRequestMsgData);
    }
}