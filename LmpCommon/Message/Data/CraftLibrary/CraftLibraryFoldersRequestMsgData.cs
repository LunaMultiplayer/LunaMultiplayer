using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryFoldersRequestMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryFoldersRequestMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.FoldersRequest;

        public override string ClassName { get; } = nameof(CraftLibraryFoldersRequestMsgData);
    }
}
