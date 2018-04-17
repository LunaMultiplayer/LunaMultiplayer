using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminRestartServerMsgData : AdminBaseMsgData
    {        
        /// <inheritdoc />
        internal AdminRestartServerMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.RestartServer;
        public override string ClassName { get; } = nameof(AdminRestartServerMsgData);
    }
}
