using LmpCommon;
using LmpCommon.Enums;
using Server.Events;
using Server.Context;
using Server.Settings.Structures;
using Server.System;
using System;
using System.IO;

namespace Server.Log
{
    /// <summary>
    /// Server-side logger that mirrors every console line into a daily <c>lmpserver_*.log</c>
    /// file under <see cref="LogFolder"/>.
    ///
    /// Writes go through a single persistent <see cref="StreamWriter"/> kept open for the
    /// lifetime of the process (or until the file is rotated by reassigning
    /// <see cref="LogFilename"/>). The previous implementation funneled every log line
    /// through <see cref="FileHandler.AppendToFile"/>, which performed a full
    /// <c>open → write → fsync → close</c> cycle per call. On Linux container hosts that
    /// pattern dirties one or more page-cache pages on every line and inflates the cgroup
    /// RSS that hosting panels report. Keeping the stream open and flushing per line gives
    /// us identical "operator can tail the log live" semantics with a fraction of the
    /// page-cache churn and zero per-line FileStream/StreamWriter allocations.
    ///
    /// Thread safety: every public log method ultimately calls <see cref="AfterPrint"/> on
    /// <see cref="Singleton"/>, and that method takes <see cref="WriteLock"/> before
    /// touching the writer. <see cref="StreamWriter"/> is not thread-safe and the server
    /// runs many message-handler tasks concurrently, so a single write lock is the
    /// simplest correct synchronization.
    /// </summary>
    public class LunaLog : BaseLogger
    {
        private static readonly BaseLogger Singleton = new LunaLog();
        public static string LogFolder = Path.Combine(ServerContext.DataDirectory, "logs");

        /// <summary>
        /// Serializes all access to <see cref="_writer"/>, <see cref="_currentPath"/>, and the
        /// open/close/rotate paths.
        /// </summary>
        private static readonly object WriteLock = new object();

        /// <summary>
        /// The path the active <see cref="_writer"/> is open against. Tracked separately from
        /// <see cref="LogFilename"/> so the property setter can detect a real change vs. an
        /// idempotent reassignment.
        /// </summary>
        private static string _currentPath;

        /// <summary>
        /// Persistent writer for the current log file. <c>null</c> means file logging is
        /// disabled (initial open failed or the writer was disposed during shutdown). In
        /// that state we still echo to the console via <see cref="BaseLogger.WriteLog"/>;
        /// the file is treated as best-effort.
        /// </summary>
        private static StreamWriter _writer;

        private static string _logFilename = Path.Combine(LogFolder, $"lmpserver_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");

        /// <summary>
        /// Path of the currently active log file. Reassigning this triggers a transparent
        /// rotation: the previous writer is flushed and disposed, and a fresh writer is
        /// opened against the new path. <see cref="LogThread"/> uses this for the daily
        /// rollover.
        /// </summary>
        public static string LogFilename
        {
            get => _logFilename;
            set => SwitchToFile(value);
        }

        static LunaLog()
        {
            try
            {
                if (!FileHandler.FolderExists(LogFolder))
                    FileHandler.FolderCreate(LogFolder);
            }
            catch (Exception e)
            {
                // Without a log directory we still let the server run, but the file writer
                // below will fail to open and we fall back to console-only.
                Console.Error.WriteLine($"LunaLog: failed to ensure log folder '{LogFolder}': {e.Message}");
            }

            // Open the file *after* the folder check so OpenWriter sees a real directory.
            // If this fails we go console-only; OpenWriter will leave _writer null.
            OpenWriter(_logFilename);

            // Flush+dispose on a clean shutdown so the last few buffered bytes hit disk.
            ExitEvent.ServerClosing += CloseLog;
        }

        #region Overrides

        protected override LogLevels LogLevel => LogSettings.SettingsStore.LogLevel;
        protected override bool UseUtcTime => true;

        protected override void AfterPrint(string line)
        {
            base.AfterPrint(line);

            lock (WriteLock)
            {
                if (_writer == null) return;

                try
                {
                    _writer.WriteLine(line);
                    // Flush per line so operators can tail -f the log in real time. The cost
                    // of a single buffered-stream flush is far smaller than the previous
                    // open/write/close cycle, and matches BaseLogger's existing behaviour of
                    // making each line immediately visible on the console.
                    _writer.Flush();
                }
                catch (Exception e)
                {
                    // Critically do NOT call LunaLog.Error here: it would re-enter AfterPrint,
                    // re-take WriteLock recursively (fine, since lock is reentrant), and try
                    // the same failing write again, generating recursive failures. Surface
                    // the problem on stderr instead.
                    Console.Error.WriteLine($"LunaLog: failed to write to {_currentPath}: {e.Message}");
                }
            }
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

        #region Writer lifecycle

        /// <summary>
        /// Atomically swap the active log file. If <paramref name="newPath"/> is the same as
        /// the currently open path and the writer is healthy, this is a no-op. Otherwise the
        /// previous writer is flushed/disposed and a new one is opened.
        /// </summary>
        private static void SwitchToFile(string newPath)
        {
            lock (WriteLock)
            {
                if (string.Equals(_currentPath, newPath, StringComparison.Ordinal) && _writer != null)
                {
                    _logFilename = newPath;
                    return;
                }

                CloseWriterLocked();
                _logFilename = newPath;
                OpenWriterLocked(newPath);
            }
        }

        /// <summary>
        /// Open <paramref name="path"/> for appending, creating it if necessary. Caller must
        /// not be holding <see cref="WriteLock"/>; this overload acquires it.
        /// </summary>
        private static void OpenWriter(string path)
        {
            lock (WriteLock)
            {
                OpenWriterLocked(path);
            }
        }

        /// <summary>
        /// Inner open routine. Caller MUST hold <see cref="WriteLock"/>.
        ///
        /// <para><c>FileMode.Append</c> + <c>FileShare.ReadWrite</c> matches the previous
        /// <c>File.AppendAllText</c> semantics: existing content is preserved (so successive
        /// rotations within the same calendar second don't truncate), and external tools can
        /// tail the file or copy it while the server is running. Failures are logged to
        /// stderr only — calling LunaLog.Error here would recurse.</para>
        /// </summary>
        private static void OpenWriterLocked(string path)
        {
            try
            {
                var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                _writer = new StreamWriter(stream) { NewLine = Environment.NewLine };
                _currentPath = path;
            }
            catch (Exception e)
            {
                _writer = null;
                _currentPath = null;
                Console.Error.WriteLine($"LunaLog: failed to open log file '{path}': {e.Message}. File logging disabled.");
            }
        }

        /// <summary>
        /// Flush and dispose the current writer. Caller MUST hold <see cref="WriteLock"/>.
        /// Safe to call when <see cref="_writer"/> is already null.
        /// </summary>
        private static void CloseWriterLocked()
        {
            if (_writer == null) return;

            try
            {
                _writer.Flush();
                _writer.Dispose();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"LunaLog: failed to close log file '{_currentPath}': {e.Message}");
            }
            finally
            {
                _writer = null;
                _currentPath = null;
            }
        }

        /// <summary>
        /// Hook invoked from <see cref="ExitEvent.ServerClosing"/>. Ensures buffered log
        /// lines are flushed before the process exits.
        /// </summary>
        private static void CloseLog()
        {
            lock (WriteLock)
            {
                CloseWriterLocked();
            }
        }

        #endregion
    }
}
