using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupListResponseMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.ListResponse;
        public string[] Groups { get; set; }
        public string[] Owners { get; set; }
    }
}
