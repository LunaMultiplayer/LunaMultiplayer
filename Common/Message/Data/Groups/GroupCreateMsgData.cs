using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupCreateMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.CreateGroup;
        public string GroupName { get; set; }
    }
}
