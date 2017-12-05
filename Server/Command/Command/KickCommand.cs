using Server.Client;
using Server.Command.Command.Base;
using Server.Command.Common;
using Server.Log;
using Server.Server;

namespace Server.Command.Command
{
    public class KickCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommand(commandArgs, out var playerName, out var reason);
            reason = string.IsNullOrEmpty(reason) ? "No reason specified" : reason;

            if (playerName != "")
            {
                var player = ClientRetriever.GetClientByName(playerName);
                if (player != null)
                {
                    LunaLog.Normal($"Kicking {playerName} from the server");
                    MessageQueuer.SendConnectionEnd(player, $"Kicked from the server: {reason}");
                }
                else
                {
                    LunaLog.Normal($"Player: {playerName} not found");
                }
            }
            else
            {
                LunaLog.Error("Syntax error. Usage: /kick playername [reason]");
            }
        }
    }
}