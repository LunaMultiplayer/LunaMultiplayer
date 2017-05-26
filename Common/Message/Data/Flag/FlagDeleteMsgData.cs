using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagDeleteMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.DeleteFile;
        public string FlagName { get; set; }
    }
}