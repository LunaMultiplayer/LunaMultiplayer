using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminListRequestMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminListRequestMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.ListRequest;

        public override string ClassName { get; } = nameof(AdminListRequestMsgData);
    }
}