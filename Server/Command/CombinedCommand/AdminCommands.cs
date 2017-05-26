using System.Collections.Generic;
using LunaServer.Command.CombinedCommand.Base;
using LunaServer.Command.Command;
using LunaServer.Command.Common;
using LunaServer.Log;

namespace LunaServer.Command.CombinedCommand
{
    public class AdminCommands : CombinedCommandBase
    {
        private static readonly AdminAddCommand AdminAddCommand = new AdminAddCommand();
        private static readonly AdminShowCommand AdminShowCommand = new AdminShowCommand();
        private static readonly AdminRemoveCommand AdminRemoveCommand = new AdminRemoveCommand();

        public override void HandleCommand(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommand(commandArgs, out var func, out var playerName);

            switch (func)
            {
                default:
                    LunaLog.Normal("Undefined function. Usage: /admin [add|del] playername or /admin show");
                    break;
                case "add":
                    AdminAddCommand.Execute(playerName);
                    break;
                case "del":
                    AdminRemoveCommand.Execute(playerName);
                    break;
                case "show":
                    AdminShowCommand.Execute("");
                    break;
            }
        }

        public static IEnumerable<string> Retrieve()
        {
            return AdminAddCommand.Retrieve();
        }
    }
}