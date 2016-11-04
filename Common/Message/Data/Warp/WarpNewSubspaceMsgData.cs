using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpNewSubspaceMsgData : WarpBaseMsgData
    {
        public override WarpMessageType WarpMessageType => WarpMessageType.NEW_SUBSPACE;
        public string PlayerCreator { get; set; }
        public int SubspaceKey { get; set; }
        public double ServerTimeDifference { get; set; }
    }
}