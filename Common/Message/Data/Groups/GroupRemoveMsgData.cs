using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupRemoveMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.Remove;
        public string GroupName { get; set; }
    }
}
