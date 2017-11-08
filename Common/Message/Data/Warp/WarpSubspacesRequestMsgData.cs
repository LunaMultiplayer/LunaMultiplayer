using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpSubspacesRequestMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpSubspacesRequestMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.SubspacesRequest;
    }
}