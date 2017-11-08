using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Command.CombinedCommand;
using LunaServer.Context;
using LunaServer.Server;
using System.Linq;

namespace LunaServer.System
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