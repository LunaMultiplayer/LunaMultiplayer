using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Chat
{
    public class ChatMessageSender : SubSystem<ChatSystem>, IMessageSender
    {
        public void SendMessage(IMessageData messageData)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ChatCliMsg>(messageData)));
        }

        public void SendChatMsg(string text, bool relay = true)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<ChatMsgData>();
            msgData.From = SettingsSystem.CurrentSettings.PlayerName;
            msgData.Text = text;
            msgData.Relay = relay;

            System.MessageSender.SendMessage(msgData);
        }
    }
}