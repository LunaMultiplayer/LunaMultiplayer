using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Motd
{
    public class MotdRequestMsgData : MotdBaseMsgData
    {
        public override MotdMessageType MotdMessageType => MotdMessageType.Request;
    }
}