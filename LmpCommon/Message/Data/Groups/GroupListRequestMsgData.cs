using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Groups
{
    public class GroupListRequestMsgData : GroupBaseMsgData
    {
        /// <inheritdoc />
        internal GroupListRequestMsgData() { }
        public override GroupMessageType GroupMessageType => GroupMessageType.ListRequest;

        public override string ClassName { get; } = nameof(GroupListRequestMsgData);
    }
}
