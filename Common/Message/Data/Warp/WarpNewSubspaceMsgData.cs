using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpNewSubspaceMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpNewSubspaceMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.NewSubspace;
        public string PlayerCreator { get; set; }
        public int SubspaceKey { get; set; }
        public double ServerTimeDifference { get; set; }
    }
}