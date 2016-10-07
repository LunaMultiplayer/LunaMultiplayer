using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminListReplyMsgData : AdminBaseMsgData
    {
        public override AdminMessageType AdminMessageType => AdminMessageType.LIST_REPLY;
        public string[] Admins { get; set; }
    }
}