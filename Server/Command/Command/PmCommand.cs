using System.Linq;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings;

namespace Server.Command.Command
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