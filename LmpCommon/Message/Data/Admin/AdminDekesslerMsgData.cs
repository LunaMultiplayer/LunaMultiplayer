using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Admin
{
    public class AdminDekesslerMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminDekesslerMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Dekessler;
        public override string ClassName { get; } = nameof(AdminDekesslerMsgData);
    }
}
