using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagDataMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.FlagData;
        public string OwnerPlayerName { get; set; }
        public string FlagName { get; set; }
        public byte[] FlagData { get; set; }
    }
}