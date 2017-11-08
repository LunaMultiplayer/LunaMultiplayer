using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListRequestMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryListRequestMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.ListRequest;
    }
}