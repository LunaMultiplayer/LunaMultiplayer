using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatChannelMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatChannelMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ChannelMessage;

        public bool SendToAll;
        public string Channel;
        public string Text;
        
        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(SendToAll);
            if (SendToAll)
            {
                Channel = string.Empty;
            }
            lidgrenMsg.Write(Channel);
            lidgrenMsg.Write(Text);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            SendToAll = lidgrenMsg.ReadBoolean();
            Channel = lidgrenMsg.ReadString();
            Text = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(bool) + Channel.GetByteCount() + Text.GetByteCount();
        }
    }
}