using LMP.Server.Client;
using LMP.Server.Command.Command.Base;
using LMP.Server.Command.Common;
using LMP.Server.Log;
using LMP.Server.Server;

namespace LMP.Server.Command.Command
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
                    MessageQueuer.SendConnectionEnd(player, $"You were banned from the server: {reason}");

                Add(playerName);
                LunaLog.Normal($"Player '{playerName}' was banned from the server: {reason}");
            }
            else
            {
                LunaLog.Normal($"Player: {playerName} not found");
            }
        }
    }
}