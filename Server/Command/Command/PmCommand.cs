using System.Linq;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Command.Command.Base;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;

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
                    var newMessageData = new ChatPrivateMsgData
                    {
                        From = GeneralSettings.SettingsStore.ConsoleIdentifier,
                        To = client.PlayerName,
                        Text = commandArgs.Substring(client.PlayerName.Length + 1)
                    };

                    MessageQueuer.SendToClient<ChatSrvMsg>(client, newMessageData);
                }
                else
                {
                    LunaLog.Normal("Player not found!");
                }
            }
        }
    }
}