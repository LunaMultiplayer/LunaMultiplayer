using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Server.Command.CombinedCommand;
using Server.Command.Command;
using Server.Context;
using Server.Log;
using Server.Settings;

namespace Server.Command
{
    public class CommandHandler
    {
        static CommandHandler()
        {
            RegisterCommands();
        }

        public static readonly ConcurrentDictionary<string, CommandDefinition> Commands =
            new ConcurrentDictionary<string, CommandDefinition>();

        private static void RegisterCommands()
        {
            //Register the server Commands
            RegisterCommand("kick", new KickCommand().Execute, "Kicks a player from the server");
            RegisterCommand("pm", new PmCommand().Execute, "Sends a message to a player");
            RegisterCommand("ban", new BanCommands().HandleCommand, "Bans someone from the server");
            RegisterCommand("admin", new AdminCommands().HandleCommand, "Sets a player as admin/removes admin from the player");
            RegisterCommand("whitelist", new WhitelistCommands().HandleCommand, "Change the server whitelist");
            RegisterCommand("help", new DisplayHelpCommand().Execute, "Displays this help");
            RegisterCommand("say", new SayCommand().Execute, "Broadcasts a message to Clients");
            RegisterCommand("dekessler", new DekesslerCommand().Execute, "Clears out debris from the server");
            RegisterCommand("nukeksc", new NukeCommand().Execute, "Clears ALL vessels from KSC and the Runway");
            RegisterCommand("listclients", new ListClientsCommand().Execute, "Lists connected Clients");
            RegisterCommand("countclients", new CountClientsCommand().Execute, "Counts connected Clients");
            RegisterCommand("connectionstats", new ConnectionStatsCommand().Execute, "Displays network traffic usage");
        }

        /// <summary>
        /// We receive the console inputs with a pipe
        /// </summary>
        public async void ThreadMain()
        {
            try
            {
                while (ServerContext.ServerRunning)
                {
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        continue;
                    }
                    LunaLog.Normal($"Command input: {input}");
                    if (!string.IsNullOrEmpty(input))
                    {
                        if (input.StartsWith("/"))
                        {
                            HandleServerInput(input.Substring(1));
                        }
                        else
                        {
                            Commands["say"].Func(input);
                        }
                    }
                    await Task.Delay(GeneralSettings.SettingsStore.MainTimeTick);
                }
            }
            catch (Exception e)
            {
                if (ServerContext.ServerRunning)
                {
                    LunaLog.Fatal($"Error in command handler thread, Exception: {e}");
                    throw;
                }
            }
        }

        public static void HandleServerInput(string input)
        {
            var commandPart = input;
            var argumentPart = "";
            if (commandPart.Contains(" "))
            {
                if (commandPart.Length > commandPart.IndexOf(' ') + 1)
                    argumentPart = commandPart.Substring(commandPart.IndexOf(' ') + 1);
                commandPart = commandPart.Substring(0, commandPart.IndexOf(' '));
            }
            if (commandPart.Length > 0)
                if (Commands.ContainsKey(commandPart))
                    try
                    {
                        Commands[commandPart].Func(argumentPart);
                    }
                    catch (Exception e)
                    {
                        LunaLog.Error($"Error handling server command {commandPart}, Exception {e}");
                    }
                else
                    LunaLog.Normal($"Unknown server command: {commandPart}");
        }

        private static void RegisterCommand(string command, Action<string> func, string description)
        {
            var cmd = new CommandDefinition(command, func, description);
            if (!Commands.ContainsKey(command))
                Commands.TryAdd(command, cmd);
        }
    }
}