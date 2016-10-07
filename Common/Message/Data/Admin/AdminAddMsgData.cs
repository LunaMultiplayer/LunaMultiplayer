using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminAddMsgData : AdminBaseMsgData
    {
        public override AdminMessageType AdminMessageType => AdminMessageType.ADD;
        public string PlayerName { get; set; }
    }
}