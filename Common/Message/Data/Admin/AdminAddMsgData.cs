using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminAddMsgData : AdminBaseMsgData
    {        
        /// <inheritdoc />
        internal AdminAddMsgData() { }

        public override AdminMessageType AdminMessageType => AdminMessageType.Add;
        public string PlayerName { get; set; }
    }
}