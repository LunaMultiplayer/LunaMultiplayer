using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminListRequestMsgData : AdminBaseMsgData
    {
        public override AdminMessageType AdminMessageType => AdminMessageType.ListRequest;
    }
}