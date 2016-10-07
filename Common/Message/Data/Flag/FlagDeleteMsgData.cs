using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagDeleteMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.DELETE_FILE;
        public string FlagName { get; set; }
    }
}