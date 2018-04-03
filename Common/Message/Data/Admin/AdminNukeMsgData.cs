using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminNukeMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminNukeMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Nuke;
        public override string ClassName { get; } = nameof(AdminNukeMsgData);
    }
}
