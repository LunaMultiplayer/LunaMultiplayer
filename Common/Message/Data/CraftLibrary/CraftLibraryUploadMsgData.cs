using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryUploadMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.UploadFile;
        public CraftType UploadType { get; set; }
        public string UploadName { get; set; }
        public byte[] CraftData { get; set; }
    }
}