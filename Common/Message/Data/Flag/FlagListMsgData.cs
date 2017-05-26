using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Flag
{
    public class FlagListMsgData : FlagBaseMsgData
    {
        public override FlagMessageType FlagMessageType => FlagMessageType.List;
        public string[] FlagFileNames { get; set; }
        public string[] FlagOwners { get; set; }
        public string[] FlagShaSums { get; set; }
    }
}