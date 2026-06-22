using LmpCommon;
using LmpCommon.Enums;
using Server.Context;
using Server.Settings.Structures;
using Server.System;
using System;
using System.IO;

namespace Server.Log
{
    public class LunaLog : BaseLogger
    {
        private static readonly BaseLogger Singleton = new LunaLog();

        static LunaLog()
        {
            if (!FileHandler.FolderExists(LogFolder))
                FileHandler.FolderCreate(LogFolder);
        }

        public static string LogFolder = Path.Combine(ServerContext.DataDirectory, "logs");

        public static string LogFilename = Path.Combine(LogFolder, $"lmpserver_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

        #region Overrides

        protected override LogLevels LogLevel => LogSettings.SettingsStore.LogLevel;
        protected override bool UseUtcTime => true;

        protected override void AfterPrint(string line)
        {
            base.AfterPrint(line);
            FileHandler.AppendToFile(LogFilename, line + Environment.NewLine);
        }

        #endregion

        #region Public methods

        public new static void NetworkVerboseDebug(string message)
        {
            Singleton.NetworkVerboseDebug(message);
        }

        public new static void NetworkDebug(string message)
        {
            Singleton.NetworkDebug(message);
        }

        public new static void Debug(string message)
        {
            Singleton.Debug(message);
        }

        public new static void Warning(string message)
        {
            Singleton.Warning(message);
        }

        public new static void Info(string message)
        {
            Singleton.Info(message);
        }

        public new static void Normal(string message)
        {
            Singleton.Normal(message);
        }

        public new static void Error(string message)
        {
            Singleton.Error(message);
        }

        public new static void Fatal(string message)
        {
            Singleton.Fatal(message);
        }

        public new static void ChatMessage(string message)
        {
            Singleton.ChatMessage(message);
        }

        #endregion
    }
}
