using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryDeleteMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.DELETE_FILE;
        public string PlayerName { get; set; }
        public CraftType CraftType { get; set; }
        public string CraftName { get; set; }
    }
}