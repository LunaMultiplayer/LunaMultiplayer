using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryRespondMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryRespondMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.RespondFile;
        public string CraftOwner { get; set; }
        public CraftType RequestedType { get; set; }
        public string RequestedName { get; set; }
        public bool HasCraft { get; set; }
        public byte[] CraftData { get; set; }
    }
}