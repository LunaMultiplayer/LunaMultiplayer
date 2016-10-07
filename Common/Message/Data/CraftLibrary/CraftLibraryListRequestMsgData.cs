using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.CraftLibrary
{
    public class CraftLibraryListRequestMsgData : CraftLibraryBaseMsgData
    {
        public override CraftMessageType CraftMessageType => CraftMessageType.LIST_REQUEST;
    }
}