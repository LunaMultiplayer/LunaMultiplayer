using LunaCommon.Flag;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagDataMsgData : FlagBaseMsgData
    {
        /// <inheritdoc />
        internal FlagDataMsgData() { }
        public override FlagMessageType FlagMessageType => FlagMessageType.FlagData;
        public FlagInfo Flag { get; set; }
    }
}