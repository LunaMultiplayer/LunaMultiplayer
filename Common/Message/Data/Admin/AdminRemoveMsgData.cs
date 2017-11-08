using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminRemoveMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminRemoveMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Remove;
        public string PlayerName { get; set; }
    }
}