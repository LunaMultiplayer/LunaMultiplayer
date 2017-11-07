using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagListRequestMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.ListRequest;
    }
}