using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminRemoveMsgData : AdminBaseMsgData
    {
        public override AdminMessageType AdminMessageType => AdminMessageType.REMOVE;
        public string PlayerName { get; set; }
    }
}