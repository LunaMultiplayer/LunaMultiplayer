using LMP.Server.Client;
using LMP.Server.Command.CombinedCommand;
using LMP.Server.Context;
using LMP.Server.Server;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Server;
using System.Linq;

namespace LMP.Server.System
{
    public class AdminSystemSender
    {
        public static void SendAdminList(ClientStructure client)
        {
            var newMessage = ServerContext.ServerMessageFactory.CreateNewMessageData<AdminListReplyMsgData>();
            newMessage.Admins = AdminCommands.Admins.ToArray();

            MessageQueuer.SendToClient<AdminSrvMsg>(client, newMessage);
        }

        public static void NotifyPlayersNewAdmin(string playerName)
        {
            var newMessage = ServerContext.ServerMessageFactory.CreateNewMessageData<AdminAddMsgData>();
            newMessage.PlayerName = playerName;

            MessageQueuer.SendToAllClients<AdminSrvMsg>(newMessage);
        }

        public static void NotifyPlayersRemovedAdmin(string playerName)
        {
            var newMessage = ServerContext.ServerMessageFactory.CreateNewMessageData<AdminRemoveMsgData>();
            newMessage.PlayerName = playerName;

            MessageQueuer.SendToAllClients<AdminSrvMsg>(newMessage);
        }
    }
}