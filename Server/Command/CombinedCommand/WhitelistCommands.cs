using System.Collections.Generic;
using LunaServer.Command.CombinedCommand.Base;
using LunaServer.Command.Command;
using LunaServer.Command.Common;
using LunaServer.Log;

namespace LunaServer.Command.CombinedCommand
{
    public class WhitelistCommands : CombinedCommandBase
    {
        private static readonly WhitelistAddCommand WhitelistAddCommand = new WhitelistAddCommand();
        private static readonly WhitelistShowCommand WhitelistShowCommand = new WhitelistShowCommand();
        private static readonly WhitelistRemoveCommand WhitelistRemoveCommand = new WhitelistRemoveCommand();

        public override void HandleCommand(string commandArgs)
        {
            string playerName;
            string func;
            CommandSystemHelperMethods.SplitCommand(commandArgs, out func, out playerName);

            switch (func)
            {
                default:
                    LunaLog.Debug("Undefined function. Usage: /whitelist [add|del] playername or /whitelist show");
                    break;
                case "add":
                    WhitelistAddCommand.Add(playerName);
                    break;
                case "del":
                    WhitelistRemoveCommand.Remove(playerName);
                    break;
                case "show":
                    WhitelistShowCommand.Retrieve();
                    break;
            }
        }

        public static IEnumerable<string> Retrieve()
        {
            return WhitelistAddCommand.Retrieve();
        }
    }
}