using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupListRequestMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.ListRequest;
    }
}
