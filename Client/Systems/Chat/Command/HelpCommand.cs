using System.Collections.Generic;

namespace LunaClient.Systems.Chat.Command
{
    public class HelpCommand
    {
        public void DisplayHelp(string commandArgs)
        {
            var commands = new List<ChatCommand>();
            var longestName = 0;
            foreach (var cmd in SystemsContainer.Get<ChatSystem>().RegisteredChatCommands.Values)
            {
                commands.Add(cmd);
                if (cmd.Name.Length > longestName)
                    longestName = cmd.Name.Length;
            }
            commands.Sort();
            foreach (var cmd in commands)
            {
                var helpText = $"{cmd.Name.PadRight(longestName)} - {cmd.Description}";
                SystemsContainer.Get<ChatSystem>().PrintToSelectedChannel(helpText);
            }
        }
    }
}