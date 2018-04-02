using Server.Command.CombinedCommand.Base;
using Server.Command.Command;
using System.Collections.Generic;

namespace Server.Command.CombinedCommand
{
    public class BanCommands : CombinedCommandBase
    {
        private static readonly BanPlayerCommand BanPlayerCommand = new BanPlayerCommand();

        public override void HandleCommand(string commandArgs)
        {
            SplitCommand(commandArgs, out var data, out var reason);

            BanPlayerCommand.Execute($"{data} {reason}");
        }
        
        public static IEnumerable<string> RetrieveBannedPlayers()
        {
            return BanPlayerCommand.Retrieve();
        }

        private static void SplitCommand(string command, out string param1, out string param2)
        {
            param2 = string.Empty;
            var splittedCommand = command.Split(' ');
            param1 = splittedCommand[0];

            if (splittedCommand.Length > 1)
                param2 = splittedCommand[1];
        }
    }
}
