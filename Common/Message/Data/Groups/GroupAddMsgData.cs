using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupAddMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.Add;
        public string GroupName { get; set; }
        public string Owner { get; set; }
    }
}
