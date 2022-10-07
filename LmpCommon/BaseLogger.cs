using LmpCommon.Enums;
using System;

namespace LmpCommon
{
    public class BaseLogger
    {
        protected virtual LogLevels LogLevel => LogLevels.Debug;
        protected virtual bool UseUtcTime => false;

        protected virtual void AfterPrint(string line)
        {
            //Implement your own after logging code
        }

        #region Private methods

        private void WriteLog(LogLevels level, string type, string message)
        {
            if (level <= LogLevel)
            {
                var output = UseUtcTime ? $"[{DateTime.UtcNow:HH:mm:ss}][{type}]: {message}" : $"[{DateTime.Now:HH:mm:ss}][{type}]: {message}";
                Console.WriteLine(output);
                AfterPrint(output);
            }
        }

        #endregion

        #region Public methods

        public void NetworkVerboseDebug(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Blue;
            WriteLog(LogLevels.VerboseNetworkDebug, "VerboseNetwork", message);
            Console.ResetColor();
        }

        public void NetworkDebug(string message)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLog(LogLevels.NetworkDebug, "NetworkDebug", message);
            Console.ResetColor();
        }

        public void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            WriteLog(LogLevels.Debug, "Debug", message);
            Console.ResetColor();
        }

        public void Warning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            WriteLog(LogLevels.Normal, "Warning", message);
            Console.ResetColor();
        }

        public void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            WriteLog(LogLevels.Normal, "Info", message);
            Console.ResetColor();
        }

        public void Normal(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            WriteLog(LogLevels.Normal, "LMP", message);
            Console.ResetColor();
        }

        public void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLog(LogLevels.Normal, "Error", message);
            Console.ResetColor();
        }

        public void Fatal(string message)
        {
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            WriteLog(LogLevels.Normal, "Fatal", message);
            Console.ResetColor();
        }

        public void ChatMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            WriteLog(LogLevels.Normal, "Chat", message);
            Console.ResetColor();
        }

        #endregion
    }
}
