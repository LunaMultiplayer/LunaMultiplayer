using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Admin
{
    public class AdminKickMsgData : AdminBanKickMsgData
    {
        /// <inheritdoc />
        internal AdminKickMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Kick;
        public override string ClassName { get; } = nameof(AdminKickMsgData);
    }
}
