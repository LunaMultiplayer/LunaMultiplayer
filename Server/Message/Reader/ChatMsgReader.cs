using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;
using System;

namespace Server.Message.Reader
{
    public class ChatMsgReader : ReaderBase
    {
        private static readonly ChatSystemReceiver ChatSystemReceiver = new ChatSystemReceiver();

        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var data = (ChatBaseMsgData)message.Data;
            if (data.From != client.PlayerName) return;

            switch (data.ChatMessageType)
            {
                case ChatMessageType.ListRequest:
                    ChatSystem.SendPlayerChatChannels(client);
                    //We don't use this message anymore so we can recycle it
                    message.Recycle();
                    break;
                case ChatMessageType.Join:
                    ChatSystemReceiver.HandleJoinMessage(client, (ChatJoinMsgData)message);
                    break;
                case ChatMessageType.Leave:
                    ChatSystemReceiver.HandleLeaveMessage(client, (ChatLeaveMsgData)message);
                    break;
                case ChatMessageType.ChannelMessage:
                    //TODO: Is the fix from message to data correct?  If so, do any of the other cases need to change?  This fix was done as a quick fix to stop the message error because message is a ChatCliMsg but data is a ChatChannelMsgData.
                    ChatSystemReceiver.HandleChannelMessage(client, (ChatChannelMsgData)data);
                    break;
                case ChatMessageType.PrivateMessage:
                    ChatSystemReceiver.HandlePrivateMessage(client, (ChatPrivateMsgData)message);
                    break;
                case ChatMessageType.ConsoleMessage:
                    ChatSystemReceiver.HandleConsoleMessage(client, (ChatConsoleMsgData)message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}