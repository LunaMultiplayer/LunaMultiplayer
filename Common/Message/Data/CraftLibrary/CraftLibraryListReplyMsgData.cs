using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListReplyMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.LIST_REPLY;
        public string[] Players { get; set; }
        public KeyValuePair<string, CraftListInfo>[] PlayerCrafts { get; set; }
    }
}