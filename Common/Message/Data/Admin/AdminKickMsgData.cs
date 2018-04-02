using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminKickMsgData : AdminBanKickMsgData
    {        
        /// <inheritdoc />
        internal AdminKickMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Kick;
        public override string ClassName { get; } = nameof(AdminKickMsgData);
    }
}
