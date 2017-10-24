using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Groups
{
    public class GroupInviteMsgData : GroupBaseMsgData
    {
        public override GroupMessageType GroupMessageType => GroupMessageType.Invite;
        public string GroupName { get; set; }
        public string AddressedTo { get; set; }
    }
}
