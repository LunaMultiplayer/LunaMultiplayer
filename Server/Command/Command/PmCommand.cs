using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Command.Command.Base;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;
using System.Linq;

namespace LunaServer.Command.Command
{
    public class PmCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            var splittedCommand = commandArgs.Trim().Split(' ');
            if (splittedCommand.Length == 2)
            {
                var playerName = splittedCommand[0];
                var client = ClientRetriever.GetAuthenticatedClients().FirstOrDefault(p => p.PlayerName == playerName);

                if (client != null)
                {
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ChatPrivateMsgData>();
                    msgData.From = GeneralSettings.SettingsStore.ConsoleIdentifier;
                    msgData.To = client.PlayerName;
                    msgData.Text = commandArgs.Substring(client.PlayerName.Length + 1);
                    
                    MessageQueuer.SendToClient<ChatSrvMsg>(client, msgData);
                }
                else
                {
                    LunaLog.Normal("Player not found!");
                }
            }
        }
    }
}