using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpSubspacesReplyMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpSubspacesReplyMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.SubspacesReply;
        public int[] SubspaceKey { get; set; }
        public double[] SubspaceTime { get; set; }
        public KeyValuePair<int, string>[] Players { get; set; }
    }
}