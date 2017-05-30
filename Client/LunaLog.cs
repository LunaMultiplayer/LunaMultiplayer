using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient
{
    /// <summary>
    /// This class implements a thread safe logger that writes into the Unity log.
    /// </summary>
    public class LunaLog
    {
        #region Helper classes

        private enum LogType
        {
            Error,
            Warning,
            Info
        }

        private class LogEntry
        {
            public LogType Type { get; }
            public string Text { get; }

            public LogEntry(LogType type, string text)
            {
                Type = type;
                Text = text;
            }
        }

        #endregion

        #region Fields & properties

        private static readonly ConcurrentQueue<LogEntry> Queue = new ConcurrentQueue<LogEntry>();

        #endregion

        #region Logging methods

        public static void LogWarning(string message)
        {
            var msg = message.Contains("[LMP]") ? message : $"[LMP]: {message}";
            if (MainSystem.IsUnityThread)
            {
                Debug.LogWarning(msg);
            }
            else
            {
                Queue.Enqueue(new LogEntry(LogType.Warning, msg));
            }
        }

        public static void LogError(string message)
        {
            var msg = message.Contains("[LMP]") ? message : $"[LMP]: {message}";
            if (MainSystem.IsUnityThread)
            {
                Debug.LogError(msg);
            }
            else
            {
                Queue.Enqueue(new LogEntry(LogType.Error, msg));
            }
        }

        public static void Log(string message)
        {
            var msg = message.Contains("[LMP]") ? message : $"[LMP]: {message}";
            if (MainSystem.IsUnityThread)
            {
                Debug.Log(msg);
            }
            else
            {
                Queue.Enqueue(new LogEntry(LogType.Info, msg));
            }
        }

        #endregion

        #region Process


        /// <summary>
        /// Call this method FROM the unity thread so it reads all the queued log messages and prints them
        /// </summary>
        public static void ProcessLogMessages()
        {
            if (!MainSystem.IsUnityThread)
            {
                throw new Exception("Cannot call ProcessLogMessages from another thread that is not the Unity thread");
            }

            while (Queue.TryDequeue(out var entry))
            {
                switch (entry.Type)
                {
                    case LogType.Error:
                        Debug.LogError(entry.Text);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(entry.Text);
                        break;
                    case LogType.Info:
                        Debug.Log(entry.Text);
                        break;
                }
            }
        }

        #endregion
    }
}
