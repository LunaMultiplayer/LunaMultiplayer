using System.Collections.Generic;
using Server.Command.CombinedCommand.Base;
using Server.Command.Command;
using Server.Log;

namespace Server.Command.CombinedCommand
{
    public class BanCommands : CombinedCommandBase
    {
        private static readonly BanIpCommand BanIpCommand = new BanIpCommand();
        private static readonly BanKeyCommand BanKeyCommand = new BanKeyCommand();
        private static readonly BanPlayerCommand BanPlayerCommand = new BanPlayerCommand();

        public override void HandleCommand(string commandArgs)
        {
            SplitCommand(commandArgs, out var type, out var data, out var reason);

            switch (type)
            {
                default:
                    LunaLog.Normal("Undefined function. Usage: /ban [key|ip|username] Data [reason]");
                    break;
                case "ip":
                    BanIpCommand.Execute($"{data} {reason}");
                    break;
                case "key":
                    BanKeyCommand.Execute($"{data} {reason}");
                    break;
                case "username":
                    BanPlayerCommand.Execute($"{data} {reason}");
                    break;
            }
        }

        public static IEnumerable<string> RetrieveBannedIps()
        {
            return BanIpCommand.Retrieve();
        }

        public static IEnumerable<string> RetrieveBannedUsernames()
        {
            return BanPlayerCommand.Retrieve();
        }

        public static IEnumerable<string> RetrieveBannedKeys()
        {
            return BanKeyCommand.Retrieve();
        }

        private static void SplitCommand(string command, out string param1, out string param2, out string param3)
        {
            param2 = "";
            param3 = "";
            var splittedCommand = command.Split(' ');
            param1 = splittedCommand[0];

            if (splittedCommand.Length > 1)
                param2 = splittedCommand[1];
            if (splittedCommand.Length > 2)
                param3 = splittedCommand[2];
        }
    }
}