using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Admin
{
    public class AdminBanMsgData : AdminBanKickMsgData
    {
        /// <inheritdoc />
        internal AdminBanMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Ban;
        public override string ClassName { get; } = nameof(AdminBanMsgData);
    }
}
