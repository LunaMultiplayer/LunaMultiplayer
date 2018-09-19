using LmpCommon.Message.Data.Chat;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Log;
using Server.Message.Base;
using Server.Server;

namespace Server.Message
{
    public class ChatMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = (ChatMsgData)message.Data;
            if (messageData.From != client.PlayerName) return;

            if (messageData.Relay)
            {
                MessageQueuer.SendToAllClients<ChatSrvMsg>(messageData);
                LunaLog.ChatMessage($"{messageData.From}: {messageData.Text}");
            }
            else //Is a PM to server msg
            {
                LunaLog.Warning($"{messageData.From}: {messageData.Text}");
            }
        }
    }
}
