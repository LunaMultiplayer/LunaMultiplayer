using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminBanMsgData : AdminBanKickMsgData
    {
        /// <inheritdoc />
        internal AdminBanMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Ban;
        public override string ClassName { get; } = nameof(AdminBanMsgData);
    }
}
