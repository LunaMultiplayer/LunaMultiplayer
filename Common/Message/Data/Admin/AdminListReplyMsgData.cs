using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminListReplyMsgData : AdminBaseMsgData
    {        
        /// <inheritdoc />
        internal AdminListReplyMsgData() { }

        public override AdminMessageType AdminMessageType => AdminMessageType.ListReply;
        public string[] Admins { get; set; }
    }
}