using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagListMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.LIST;
        public string[] FlagFileNames { get; set; }
        public string[] FlagOwners { get; set; }
        public string[] FlagShaSums { get; set; }
    }
}