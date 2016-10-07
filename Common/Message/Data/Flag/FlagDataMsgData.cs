using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagDataMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.FLAG_DATA;
        public string OwnerPlayerName { get; set; }
        public string FlagName { get; set; }
        public byte[] FlagData { get; set; }
    }
}