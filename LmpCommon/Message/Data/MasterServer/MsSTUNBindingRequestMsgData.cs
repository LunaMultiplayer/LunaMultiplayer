using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.MasterServer
{
    public class MsSTUNBindingRequestMsgData : MsBaseMsgData
    {
        /// <inheritdoc />
        internal MsSTUNBindingRequestMsgData() { }
        public override MasterServerMessageSubType MasterServerMessageSubType => MasterServerMessageSubType.STUNBindingRequest;

        public override string ClassName { get; } = nameof(MsSTUNBindingRequestMsgData);
    }
}
