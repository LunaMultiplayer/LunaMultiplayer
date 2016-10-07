using LunaServer.Client;
using LunaServer.Command.Command.Base;
using LunaServer.Command.Common;
using LunaServer.Log;
using LunaServer.Server;

namespace LunaServer.Command.Command
{
    public class BanPlayerCommand : HandledCommand
    {
        protected override string FileName => "LMPPlayerBans.txt";
        protected override object CommandLock { get; } = new object();

        public override void Execute(string commandArgs)
        {
            string playerName;
            string reason;
            CommandSystemHelperMethods.SplitCommand(commandArgs, out playerName, out reason);
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