using Server.Command.Command;
using Server.Context;
using Server.Log;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Command
{
    public class CommandHandler
    {
        static CommandHandler() => RegisterCommands();

        public static readonly ConcurrentDictionary<string, CommandDefinition> Commands =
            new ConcurrentDictionary<string, CommandDefinition>();

        private static void RegisterCommands()
        {
            //Register the server Commands
            RegisterCommand("ban", new BanPlayerCommand().Execute, "Bans someone from the server");
            RegisterCommand("changesettings", new ChangeSettingsCommand().Execute, "Changes the server settings");
            RegisterCommand("clearvessels", new ClearVesselsCommand().Execute, "Clears ALL SPECIFIED vessels from universe");
            RegisterCommand("connectionstats", new ConnectionStatsCommand().Execute, "Displays network traffic usage");
            RegisterCommand("countclients", new CountClientsCommand().Execute, "Counts connected clients");
            RegisterCommand("dekessler", new DekesslerCommand().Execute, "Clears out debris from the server");
            RegisterCommand("help", new DisplayHelpCommand().Execute, "Displays this help");
            RegisterCommand("kick", new KickCommand().Execute, "Kicks a player from the server");
            RegisterCommand("listclients", new ListClientsCommand().Execute, "Lists connected clients");
            RegisterCommand("listlocks", new ListLocksCommand().Execute, "Lists current locks");
            RegisterCommand("nukeksc", new NukeCommand().Execute, "Clears ALL vessels from KSC and the runway");
            RegisterCommand("setfunds", new SetFundsCommand().Execute, "Set funds value");
            RegisterCommand("setscience", new SetScienceCommand().Execute, "Set science value");
            RegisterCommand("restartserver", new RestartServerCommand().Execute, "Restarts the server");
            RegisterCommand("say", new SayCommand().Execute, "Broadcasts a message to clients");
        }

        /// <summary>
        /// We receive the console inputs with a pipe
        /// </summary>
        public static async void ThreadMain()
        {
            try
            {
                while (ServerContext.ServerRunning)
                {
                    var input = Console.ReadLine();
                    if (input == null)
                    {
                        LunaLog.Normal("End of stdin, stopping command listener");
                        break;
                    }
                    if (!string.IsNullOrEmpty(input))
                    {
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
                    }

                    //We only accept a command once every 500ms
                    await Task.Delay(500);
                }
            }
            catch (Exception e)
            {
                if (ServerContext.ServerRunning && !(e is ThreadAbortException))
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

        private static void RegisterCommand(string command, Func<string, bool> func, string description)
        {
            var cmd = new CommandDefinition(command, func, description);
            if (!Commands.ContainsKey(command))
                Commands.TryAdd(command, cmd);
        }
    }
}
