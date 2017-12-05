using LMP.Server.Client;
using LMP.Server.Command.Command.Base;
using LMP.Server.Context;
using LMP.Server.Log;
using LMP.Server.Server;
using LMP.Server.Settings;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server;
using System.Linq;

namespace LMP.Server.Command.Command
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