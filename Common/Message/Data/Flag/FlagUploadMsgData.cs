using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagUploadMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.UploadFile;
        public string FlagName { get; set; }
        public byte[] FlagData { get; set; }
    }
}