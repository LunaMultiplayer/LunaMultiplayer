using Server.Client;
using Server.Command.Command.Base;
using Server.Command.Common;
using Server.Log;
using Server.Server;

namespace Server.Command.Command
{
    public class BanPlayerCommand : HandledCommand
    {
        protected override string FileName => "LMPPlayerBans.txt";
        protected override object CommandLock { get; } = new object();

        public override void Execute(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommand(commandArgs, out var playerName, out var reason);
            reason = string.IsNullOrEmpty(reason) ? "No reason specified" : reason;

            if (!string.IsNullOrEmpty(playerName))
            {
                var player = ClientRetriever.GetClientByName(playerName);

                if (player != null)
                {
                    MessageQueuer.SendConnectionEnd(player, $"You were banned from the server: {reason}");
                    Add(player.UniqueIdentifier);
                    LunaLog.Normal($"Player '{playerName}' was banned from the server: {reason}");
                }
                else
                {
                    LunaLog.Normal($"Player '{playerName}' not found");
                }
            }
            else
            {
                LunaLog.Normal("Undefined function. Usage: /ban [username] [reason]");
            }
        }
    }
}
