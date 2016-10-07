using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagUploadMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.UPLOAD_FILE;
        public string FlagName { get; set; }
        public byte[] FlagData { get; set; }
    }
}