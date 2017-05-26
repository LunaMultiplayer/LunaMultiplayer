using System.Collections.Generic;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListReplyMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.ListReply;
        public string[] Players { get; set; }
        public KeyValuePair<string, CraftListInfo>[] PlayerCrafts { get; set; }
    }
}