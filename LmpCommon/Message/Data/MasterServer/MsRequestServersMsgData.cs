using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.MasterServer
{
    public class MsRequestServersMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsRequestServersMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.RequestServers;

        public override string ClassName { get; } = nameof(MsRequestServersMsgData);
    }
}
