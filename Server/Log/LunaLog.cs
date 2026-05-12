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
            LogRingBuffer.Add(LogEntry.Parse(line, DateTime.UtcNow));
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

        #region Subsystem-tagged overloads

        // These add a "[Subsystem]: message" prefix so the on-disk format matches
        // the existing inline convention (e.g. BackupCommand's "[Backup]: ..."
        // and CleanContractsCommand's "[CleanContracts]: ..."). LogEntry.Parse
        // splits the prefix back into the structured Subsystem field for the
        // ring buffer. Not provided for NetworkDebug/VerboseNetworkDebug — those
        // are transport-level firehoses where subsystem tagging is meaningless.

        public static void Debug(string subsystem, string message) => Singleton.Debug(FormatTagged(subsystem, message));
        public static void Warning(string subsystem, string message) => Singleton.Warning(FormatTagged(subsystem, message));
        public static void Info(string subsystem, string message) => Singleton.Info(FormatTagged(subsystem, message));
        public static void Normal(string subsystem, string message) => Singleton.Normal(FormatTagged(subsystem, message));
        public static void Error(string subsystem, string message) => Singleton.Error(FormatTagged(subsystem, message));
        public static void Fatal(string subsystem, string message) => Singleton.Fatal(FormatTagged(subsystem, message));
        public static void ChatMessage(string subsystem, string message) => Singleton.ChatMessage(FormatTagged(subsystem, message));

        private static string FormatTagged(string subsystem, string message)
        {
            return string.IsNullOrEmpty(subsystem) ? message : $"[{subsystem}]: {message}";
        }

        #endregion

        #region Ring buffer access

        /// <summary>
        /// Snapshot of recently logged entries, oldest first. Used by the admin
        /// dashboard (Stage 3.7) and ad-hoc inspection. Bounded by
        /// <see cref="LogRingBuffer.Capacity"/>.
        /// </summary>
        public static LogEntry[] RecentEntries() => LogRingBuffer.Snapshot();

        #endregion
    }
}
