using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryAddMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.ADD_FILE;
        public string PlayerName { get; set; }
        public CraftType UploadType { get; set; }
        public string UploadName { get; set; }
    }
}