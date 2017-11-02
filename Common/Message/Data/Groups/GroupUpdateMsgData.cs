using LunaCommon.Groups;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupUpdateMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.GroupUpdate;

        public Group Group { get; set; }
    }
}
