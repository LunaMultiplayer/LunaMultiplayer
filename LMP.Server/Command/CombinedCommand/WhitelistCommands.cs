using LMP.Server.Command.CombinedCommand.Base;
using LMP.Server.Command.Command;
using LMP.Server.Command.Common;
using LMP.Server.Log;
using System.Collections.Generic;

namespace LMP.Server.Command.CombinedCommand
{
    public class WhitelistCommands : CombinedCommandBase
    {
        private static readonly WhitelistAddCommand WhitelistAddCommand = new WhitelistAddCommand();
        private static readonly WhitelistShowCommand WhitelistShowCommand = new WhitelistShowCommand();
        private static readonly WhitelistRemoveCommand WhitelistRemoveCommand = new WhitelistRemoveCommand();

        public override void HandleCommand(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommand(commandArgs, out var func, out var playerName);

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