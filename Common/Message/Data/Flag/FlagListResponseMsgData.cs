using LunaCommon.Flag;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagListResponseMsgData : FlagBaseMsgData
    {
        /// <inheritdoc />
        internal FlagListResponseMsgData() { }
        public override FlagMessageType FlagMessageType => FlagMessageType.ListResponse;
        public FlagInfo[] FlagFiles { get; set; }
    }
}