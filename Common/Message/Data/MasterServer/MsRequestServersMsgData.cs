using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.MasterServer
{
    public class MsRequestServersMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsRequestServersMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.RequestServers;
        public string CurrentVersion { get; set; }
    }
}
