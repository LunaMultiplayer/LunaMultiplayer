using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Motd
{
    public class MotdReplyMsgData : MotdBaseMsgData
    {
        public override MotdMessageType MotdMessageType => MotdMessageType.REPLY;
        public string MessageOfTheDay { get; set; }
    }
}