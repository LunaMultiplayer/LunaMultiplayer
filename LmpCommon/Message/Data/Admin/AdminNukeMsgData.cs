using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Admin
{
    public class AdminNukeMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminNukeMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Nuke;
        public override string ClassName { get; } = nameof(AdminNukeMsgData);
    }
}
