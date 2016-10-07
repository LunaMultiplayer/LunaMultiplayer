using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpChangeSubspaceMsgData : WarpBaseMsgData
    {
        public override WarpMessageType WarpMessageType => WarpMessageType.CHANGE_SUBSPACE;
        public string PlayerName { get; set; }
        public int Subspace { get; set; }
    }
}