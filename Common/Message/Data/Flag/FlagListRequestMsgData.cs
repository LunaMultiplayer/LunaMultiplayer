using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagListRequestMsgData : FlagBaseMsgData
    {
        /// <inheritdoc />
        internal FlagListRequestMsgData() { }
        public override FlagMessageType FlagMessageType => FlagMessageType.ListRequest;

        public override string ClassName { get; } = nameof(FlagListRequestMsgData);
    }
}