using LunaCommon.Groups;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupListResponseMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.ListResponse;
        public Group[] Groups { get; set; }
    }
}
