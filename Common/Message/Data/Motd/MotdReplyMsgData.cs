using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Motd
{
    public class MotdReplyMsgData : MotdBaseMsgData
    {
        /// <inheritdoc />
        internal MotdReplyMsgData() { }
        public override MotdMessageType MotdMessageType => MotdMessageType.Reply;
        public string MessageOfTheDay { get; set; }
    }
}