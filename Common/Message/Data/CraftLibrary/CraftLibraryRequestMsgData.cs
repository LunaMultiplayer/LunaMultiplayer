using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryRequestMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.RequestFile;
        public string CraftOwner { get; set; }
        public CraftType RequestedType { get; set; }
        public string RequestedName { get; set; }
    }
}