using LunaCommon.Message.Base;
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatBaseMsgData : MessageData
    {
        public override ushort SubType => (ushort)(int)ChatMessageType;

        public virtual ChatMessageType ChatMessageType => throw new NotImplementedException();

        public string From { get; set; }
    }
}