using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Command.CombinedCommand;
using LunaServer.Server;
using System.Linq;

namespace LunaServer.System
{
    public class AdminSystemSender
    {
        public static void SendAdminList(ClientStructure client)
        {
            var newMessage = new AdminListReplyMsgData
            {
                Admins = AdminCommands.Admins.ToArray()
            };

            MessageQueuer.SendToClient<AdminSrvMsg>(client, newMessage);
        }

        public static void NotifyPlayersNewAdmin(string playerName)
        {
            MessageQueuer.SendToAllClients<AdminSrvMsg>(new AdminAddMsgData {PlayerName = playerName});
        }

        public static void NotifyPlayersRemovedAdmin(string playerName)
        {
            MessageQueuer.SendToAllClients<AdminSrvMsg>(new AdminRemoveMsgData {PlayerName = playerName});
        }
    }
}