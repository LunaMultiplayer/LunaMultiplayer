using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryRespondMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.RESPOND_FILE;
        public string CraftOwner { get; set; }
        public CraftType RequestedType { get; set; }
        public string RequestedName { get; set; }
        public bool HasCraft { get; set; }
        public byte[] CraftData { get; set; }
    }
}