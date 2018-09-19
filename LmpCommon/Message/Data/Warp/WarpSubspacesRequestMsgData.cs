using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Warp
{
    public class WarpSubspacesRequestMsgData : WarpBaseMsgData
    {
        /// <inheritdoc />
        internal WarpSubspacesRequestMsgData() { }
        public override WarpMessageType WarpMessageType => WarpMessageType.SubspacesRequest;

        public override string ClassName { get; } = nameof(WarpSubspacesRequestMsgData);
    }
}