using System;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Message.Reader.Base;
using LunaServer.System;

namespace LunaServer.Message.Reader
{
    public class ChatMsgReader : ReaderBase
    {
        private static readonly ChatSystemReceiver ChatSystemReceiver = new ChatSystemReceiver();

        public override void HandleMessage(ClientStructure client, IMessageData message)
        {
            var data = (ChatBaseMsgData) message;
            if (data.From != client.PlayerName) return;

            switch (data.ChatMessageType)
            {
                case ChatMessageType.LIST_REQUEST:
                    ChatSystem.SendPlayerChatChannels(client);
                    break;
                case ChatMessageType.JOIN:
                    ChatSystemReceiver.HandleJoinMessage(client, (ChatJoinMsgData) message);
                    break;
                case ChatMessageType.LEAVE:
                    ChatSystemReceiver.HandleLeaveMessage(client, (ChatLeaveMsgData) message);
                    break;
                case ChatMessageType.CHANNEL_MESSAGE:
                    ChatSystemReceiver.HandleChannelMessage(client, (ChatChannelMsgData) message);
                    break;
                case ChatMessageType.PRIVATE_MESSAGE:
                    ChatSystemReceiver.HandlePrivateMessage(client, (ChatPrivateMsgData) message);
                    break;
                case ChatMessageType.CONSOLE_MESSAGE:
                    ChatSystemReceiver.HandleConsoleMessage(client, (ChatConsoleMsgData) message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}