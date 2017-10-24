using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupKickMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.Kick;
        public string GroupName { get; set; }
        public string Player { get; set; }
    }
}
