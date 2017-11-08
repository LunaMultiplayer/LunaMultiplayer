using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListReplyMsgData : CraftLibraryBaseMsgData
    {
        /// <inheritdoc />
        internal CraftLibraryListReplyMsgData() { }
        public override CraftMessageType CraftMessageType => CraftMessageType.ListReply;
        public string[] Players { get; set; }
        public KeyValuePair<string, CraftListInfo>[] PlayerCrafts { get; set; }
    }
}