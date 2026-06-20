using Server.Events;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Server.Log
{
    /// <summary>
    /// Writes craft create/remove audit entries to <c>logs/CraftCreationAndRemoval.txt</c>.
    /// The file is reset on startup, kept open for the process lifetime, and closed on
    /// <see cref="ExitEvent.ServerClosing"/>.
    /// </summary>
    public static class CraftCreationAndRemovalLog
    {
        private static readonly string LogFilePath = Path.Combine(LunaLog.LogFolder, "CraftCreationAndRemoval.txt");

        /// <summary>
        /// Serializes access to <see cref="_writer"/>.
        /// </summary>
        private static readonly object WriteLock = new object();

        /// <summary>
        /// Persistent writer. If opening fails, this remains null and logging is skipped.
        /// </summary>
        private static StreamWriter _writer;

        static CraftCreationAndRemovalLog()
        {
            try
            {
                if (!System.FileHandler.FolderExists(LunaLog.LogFolder))
                    System.FileHandler.FolderCreate(LunaLog.LogFolder);

                // Reset per run; allow read access for live tailing.
                var stream = new FileStream(LogFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                _writer = new StreamWriter(stream) { NewLine = Environment.NewLine };

                _writer.WriteLine("# Craft Creation and Removal audit log");
                _writer.WriteLine($"# Server started at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                _writer.WriteLine("# Format: [Timestamp UTC] Vessel <GUID> (<Vessel Name>) created/removed by player <Player> (<Reason>)");
                _writer.Flush();

                ExitEvent.ServerClosing += CloseLog;
            }
            catch (Exception e)
            {
                // Audit logging is best-effort and must not crash startup.
                LunaLog.Error($"Failed to initialize CraftCreationAndRemoval.txt: {e.Message}");
                _writer = null;
            }
        }

        /// <summary>
        /// Forces static initialization so the audit file is reset at startup.
        /// </summary>
        public static void Initialize()
        {
            // Touching a static member triggers static initialization.
            _ = LogFilePath;
        }

        /// <summary>
        /// Record that a brand-new vessel was registered on the server.
        /// </summary>
        public static void LogCreated(Guid vesselId, string vesselName, string playerName, string reason)
        {
            WriteLine("created", vesselId, vesselName, playerName, reason);
        }

        /// <summary>
        /// Record that a vessel was removed from the server.
        /// </summary>
        public static void LogRemoved(Guid vesselId, string vesselName, string playerName, string reason)
        {
            WriteLine("removed", vesselId, vesselName, playerName, reason);
        }

        /// <summary>
        /// Extracts the vessel-level <c>name = ...</c> field from raw config text.
        /// Returns null when no vessel name is present.
        /// </summary>
        private static readonly Regex VesselNameRegex = new Regex(
            @"(?:^|\n)\s*name\s*=\s*(?<value>.*?)\s*(?:\r|\n|$)",
            RegexOptions.Compiled);

        public static string ExtractVesselName(string vesselConfigNodeText)
        {
            if (string.IsNullOrEmpty(vesselConfigNodeText)) return null;

            // Scan only the header to avoid part-level "name = ..." matches.
            var scanLength = Math.Min(vesselConfigNodeText.Length, 2048);
            var header = vesselConfigNodeText.Substring(0, scanLength);

            var match = VesselNameRegex.Match(header);
            return match.Success ? match.Groups["value"].Value : null;
        }

        private static void WriteLine(string action, Guid vesselId, string vesselName, string playerName, string reason)
        {
            var safeName = string.IsNullOrWhiteSpace(vesselName) ? "Unknown" : vesselName.Trim();
            var safePlayer = string.IsNullOrWhiteSpace(playerName) ? "Unknown" : playerName.Trim();
            var safeReason = string.IsNullOrWhiteSpace(reason) ? "Unknown" : reason.Trim();

            var line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] Vessel {vesselId} ({safeName}) {action} by player {safePlayer} ({safeReason})";

            lock (WriteLock)
            {
                if (_writer == null) return;

                try
                {
                    _writer.WriteLine(line);
                    // Flush per entry for live troubleshooting.
                    _writer.Flush();
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Failed to append to CraftCreationAndRemoval.txt: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Flushes and closes the audit writer on shutdown.
        /// </summary>
        private static void CloseLog()
        {
            lock (WriteLock)
            {
                if (_writer == null) return;

                try
                {
                    _writer.Flush();
                    _writer.Dispose();
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Failed to close CraftCreationAndRemoval.txt: {e.Message}");
                }
                finally
                {
                    _writer = null;
                }
            }
        }
    }
}
