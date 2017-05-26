using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Warp
{
    public class WarpSubspacesRequestMsgData : WarpBaseMsgData
    {
        public override WarpMessageType WarpMessageType => WarpMessageType.SubspacesRequest;
    }
}