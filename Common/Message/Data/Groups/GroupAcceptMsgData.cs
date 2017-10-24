using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupAcceptMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.Accept;
        public string GroupName { get; set; }
        public string AddressedTo { get; set; }
    }
}
