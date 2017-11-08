using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryAddMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryAddMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.AddFile;
        public CraftType UploadType { get; set; }
        public string UploadName { get; set; }
    }
}