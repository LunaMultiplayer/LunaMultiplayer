using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupListRequestMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupListRequestMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.ListRequest;

        public override string ClassName { get; } = nameof(GroupListRequestMsgData);
    }
}
