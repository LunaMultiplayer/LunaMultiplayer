using System;
using System.IO;
using Server.Settings;
using Server.System;

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
            $"lmpserver {DateTime.Now:yyyy-MM-dd HH-mm-ss}.log");

        #region Private methods

        private static void WriteLog(LogLevels level, string message, bool sendToConsole)
        {
            if (level >= GeneralSettings.SettingsStore.LogLevel)
            {
                var output = GeneralSettings.SettingsStore.UseUtcTimeInLog
                    ? $"[{DateTime.UtcNow:HH:mm:ss}][{level}] : {message}"
                    : $"[{DateTime.Now:HH:mm:ss}][{level}] : {message}";

                if (sendToConsole)
                {
                    Console.WriteLine(output);
                    ChatSystem.SendConsoleMessageToAdmins(output);
                }

                FileHandler.AppendToFile(LogFilename, output + Environment.NewLine);
            }
        }

        #endregion

        #region Public methods

        public static void WriteToLog(string message)
        {
            WriteLog(LogLevels.Info, message, false);
        }

        public static void NetworkDebug(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLog(LogLevels.Debug, message, true);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            WriteLog(LogLevels.Debug, message, true);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLog(LogLevels.Warning, message, true);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Normal(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            WriteLog(LogLevels.Info, message, true);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            WriteLog(LogLevels.Error, message, true);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void Fatal(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLog(LogLevels.Fatal, message, true);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ChatMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLog(LogLevels.Chat, message, true);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        #endregion
    }
}