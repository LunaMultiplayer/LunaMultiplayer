using Server.Settings.Structures;
using Server.System;
using System;
using System.IO;

namespace Server.Log
{
    public class LunaLog
    {
        static LunaLog()
        {
            if (!FileHandler.FolderExists(LogFolder))
                FileHandler.FolderCreate(LogFolder);
        }

        public static string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        public static string LogFilename = Path.Combine(LogFolder,
            $"lmpserver_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

        #region Private methods

        private static void WriteLog(LogLevels level, string type, string message, bool sendToConsole)
        {
            if (level <= LogSettings.SettingsStore.LogLevel)
            {
                var output = LogSettings.SettingsStore.UseUtcTimeInLog
                    ? $"[{DateTime.UtcNow:HH:mm:ss}]" : $"[{DateTime.Now:HH:mm:ss}]";

                if (!string.IsNullOrEmpty(type))
                    output += $"[{type}]";

                output += $": {message}";

                if (sendToConsole)
                {
                    Console.WriteLine(output);
                }

                FileHandler.AppendToFile(LogFilename, output + Environment.NewLine);
            }
        }

        #endregion

        #region Public methods

        public static void NetworkVerboseDebug(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteLog(LogLevels.VerboseNetworkDebug, "VerboseNetwork", message, true);
            Console.ResetColor();
        }

        public static void NetworkDebug(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLog(LogLevels.NetworkDebug, "NetworkDebug", message, true);
            Console.ResetColor();
        }

        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLog(LogLevels.Debug, "Debug", message, true);
            Console.ResetColor();
        }

        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLog(LogLevels.Normal, "Warning", message, true);
            Console.ResetColor();
        }

        public static void Info(string message)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLog(LogLevels.Normal, "Info", message, true);
            Console.ResetColor();
        }

        public static void Normal(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            WriteLog(LogLevels.Normal, string.Empty, message, true);
            Console.ResetColor();
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLog(LogLevels.Normal, "Error", message, true);
            Console.ResetColor();
        }

        public static void Fatal(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLog(LogLevels.Normal, "Fatal", message, true);
            Console.ResetColor();
        }

        public static void ChatMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLog(LogLevels.Normal, "Chat", message, true);
            Console.ResetColor();
        }

        #endregion
    }
}
