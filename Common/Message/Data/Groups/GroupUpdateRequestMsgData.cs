using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupUpdateRequestMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.UpdateRequest;

        public string GroupName;
    }
}
