using LMP.Server.Command.Command.Base;
using LMP.Server.Log;
using System.Collections.Generic;

namespace LMP.Server.Command.Command
{
    public class DisplayHelpCommand : SimpleCommand
    {
        public override void Execute(string commandArgs)
        {
            var commands = new List<CommandDefinition>();
            var longestName = 0;
            foreach (var cmd in CommandHandler.Commands.Values)
            {
                commands.Add(cmd);
                if (cmd.Name.Length > longestName)
                    longestName = cmd.Name.Length;
            }
            foreach (var cmd in commands)
                LunaLog.Normal($"{cmd.Name.PadRight(longestName)} - {cmd.Description}");
        }
    }
}