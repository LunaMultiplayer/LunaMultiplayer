using System.Collections.Generic;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpSubspacesReplyMsgData : WarpBaseMsgData
    {
        public override WarpMessageType WarpMessageType => WarpMessageType.SUBSPACES_REPLY;
        public int[] SubspaceKey { get; set; }
        public double[] SubspaceTime { get; set; }
        public KeyValuePair<int, string>[] Players { get; set; }
    }
}