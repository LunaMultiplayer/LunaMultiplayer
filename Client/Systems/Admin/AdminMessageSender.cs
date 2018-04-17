using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.Admin
{
    public class AdminMessageSender : SubSystem<AdminSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<AdminCliMsg>(msg)));
        }

        public void SendBanPlayerMsg(string playerName, string reason)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<AdminBanMsgData>();
            msgData.AdminPassword = System.AdminPassword;
            msgData.PlayerName = playerName;
            msgData.Reason = reason;

            SendMessage(msgData);
        }

        public void SendKickPlayerMsg(string playerName, string reason)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<AdminKickMsgData>();
            msgData.AdminPassword = System.AdminPassword;
            msgData.PlayerName = playerName;
            msgData.Reason = reason;

            SendMessage(msgData);
        }

        public void SendNukeMsg()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<AdminNukeMsgData>();
            msgData.AdminPassword = System.AdminPassword;

            SendMessage(msgData);
        }

        public void SendDekesslerMsg()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<AdminDekesslerMsgData>();
            msgData.AdminPassword = System.AdminPassword;

            SendMessage(msgData);
        }

        public void SendServerRestartMsg()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<AdminRestartServerMsgData>();
            msgData.AdminPassword = System.AdminPassword;

            SendMessage(msgData);
        }
    }
}
