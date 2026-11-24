using Expansions;
using LmpClient.Network;
using LmpClient.Systems.SettingsSys;
using LmpCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LmpClient.Systems.Scenario
{
    /// <summary>
    /// Writes diagnostics for the scenario synchronization pipeline.
    ///
    /// <see cref="ScenarioSystem"/> decides *when* something is worth reporting, this class decides
    /// *what* context is captured and *where* it is written. A short line always goes to KSP.log so
    /// it can be correlated with the surrounding LMP messages, while the full context block is
    /// appended to ScenarioSyncDebug.log in the KSP install folder (same approach as NullScenario.log).
    ///
    /// Every report is throttled per signature because the send routine runs every 30 seconds and an
    /// unattended loop would otherwise flood both logs.
    /// </summary>
    internal static class ScenarioSyncDiagnostics
    {
        private const string LogFileName = "ScenarioSyncDebug.log";
        private const int MaxReportsPerSignature = 3;
        private const string Separator = "================================================================================";

        private static readonly object FileLock = new object();
        private static readonly ConcurrentDictionary<string, int> ReportCounts = new ConcurrentDictionary<string, int>();
        private static DateTime _enabledAtUtc = DateTime.MinValue;

        /// <summary>
        /// Gated on <see cref="SettingStructure.ScenarioSyncDiagnosticsEnabled"/>. When off, nothing is
        /// written to <see cref="LogFileName"/> and only short, throttled warnings for genuine problems
        /// (anomalies, a stuck reply) reach KSP.log. Any throw while reading settings counts as off.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                try { return SettingsSystem.CurrentSettings?.ScenarioSyncDiagnosticsEnabled ?? false; }
                catch { return false; }
            }
        }

        #region Session

        /// <summary>
        /// Called when the scenario system is enabled (right after players are synced and before the
        /// game itself is created). Records the moment so later reports can show how long the client
        /// has been waiting, and dumps the scenario type inventory of this particular install, which
        /// is what tells us whether a problem is caused by a mod only this player has.
        /// </summary>
        public static void SessionStarted(Func<IDictionary<string, Type>> knownScenarioTypes)
        {
            ReportCounts.Clear();
            _enabledAtUtc = DateTime.UtcNow;

            if (!Enabled) return;

            Write("SCENARIO SYSTEM ENABLED", sb =>
            {
                AppendEnvironment(sb);
                AppendLoadedModules(sb);
                AppendScenarioTypeInventory(sb, knownScenarioTypes?.Invoke());
            });
        }

        public static void SessionEnded()
        {
            if (Enabled)
                Write("SCENARIO SYSTEM DISABLED", AppendEnvironment);

            ReportCounts.Clear();
            _enabledAtUtc = DateTime.MinValue;
        }

        #endregion

        #region Reporting

        /// <summary>
        /// Reports an exception together with the full state of the scenario pipeline.
        /// </summary>
        /// <param name="context">Where the failure happened, e.g. "ParseModulesToConfigNodes/ResourceScenario".</param>
        /// <param name="exception">The exception that was caught.</param>
        /// <param name="extraDetail">Optional extra lines specific to the call site.</param>
        public static void Failure(string context, Exception exception, Action<StringBuilder> extraDetail = null)
        {
            //Every call site already logs its own error to KSP.log, so with diagnostics off there is nothing to add
            if (!Enabled) return;
            if (!ShouldReport($"failure|{context}|{exception?.GetType().Name}")) return;

            LunaLog.LogError($"[LMP]: Scenario sync failure in {context} — {Summary()}. See {LogFileName} in your KSP folder. {exception}");

            Write($"FAILURE — {context}", sb =>
            {
                AppendEnvironment(sb);
                extraDetail?.Invoke(sb);
                AppendLoadedModules(sb);
                sb.AppendLine("--- Exception ---");
                sb.AppendLine(exception?.ToString() ?? "(null exception)");
            });
        }

        public static void RequestSent()
        {
            if (!Enabled) return;
            if (!ShouldReport("request-sent")) return;

            LunaLog.Log($"[LMP]: Requesting scenario modules from the server. {TransportSummary()} ({Summary()})");
            Write("SCENARIO REQUEST SENT", sb =>
            {
                sb.AppendLine($"  {TransportSummary()}");
                AppendEnvironment(sb);
            });
        }

        public static void ReplyReceived(int scenarioCount, int totalBytes, string modules)
        {
            if (!Enabled) return;

            LunaLog.Log($"[LMP]: Received {scenarioCount} scenario modules ({totalBytes} bytes): {modules} ({Summary()})");
            Write("SCENARIO REPLY RECEIVED", sb =>
            {
                sb.AppendLine($"  Count: {scenarioCount}");
                sb.AppendLine($"  Total bytes: {totalBytes}");
                sb.AppendLine($"  Modules: {modules}");
                sb.AppendLine($"  {TransportSummary()}");
                AppendEnvironment(sb);
            });
        }

        public static void WaitingForReply()
        {
            if (!ShouldReport("waiting-for-reply")) return;

            LunaLog.LogWarning($"[LMP]: Still waiting for the server scenario reply. {TransportSummary()} ({Summary()})");
            if (!Enabled) return;

            Write("WAITING FOR SCENARIO REPLY", sb =>
            {
                sb.AppendLine($"  {TransportSummary()}");
                AppendEnvironment(sb);
            });
        }

        /// <summary>
        /// Reports a state that is not an exception but should never normally happen, such as a
        /// scenario the client cannot resolve or a game object that is still null.
        /// </summary>
        public static void Anomaly(string signature, string message)
        {
            if (!ShouldReport($"anomaly|{signature}")) return;

            LunaLog.LogWarning($"[LMP]: Scenario sync anomaly — {message} ({Summary()})");
            if (!Enabled) return;

            Write($"ANOMALY — {signature}", sb =>
            {
                sb.AppendLine($"  Message: {message}");
                AppendEnvironment(sb);
                AppendLoadedModules(sb);
            });
        }

        /// <summary>
        /// Single line summary suitable for appending to an ordinary KSP.log message.
        /// </summary>
        public static string Summary()
        {
            var sb = new StringBuilder();
            sb.Append("networkState=").Append(Safe(() => MainSystem.NetworkState.ToString()));
            sb.Append(" scene=").Append(Safe(() => HighLogic.LoadedScene.ToString()));
            sb.Append(" currentGame=").Append(Safe(() => HighLogic.CurrentGame == null ? "NULL" : HighLogic.CurrentGame.Mode.ToString()));
            sb.Append(" serverGameMode=").Append(Safe(() => SettingsSystem.ServerSettings.GameMode.ToString()));
            sb.Append(" sinceEnabled=").Append(Safe(SecondsSinceEnabled));
            return sb.ToString();
        }

        private static bool ShouldReport(string signature)
        {
            var count = ReportCounts.AddOrUpdate(signature, 1, (key, previous) => previous + 1);
            if (count == MaxReportsPerSignature)
                LunaLog.LogWarning($"[LMP]: Further scenario sync reports for '{signature}' will be suppressed for this session.");

            return count <= MaxReportsPerSignature;
        }

        #endregion

        #region Context blocks

        /// <summary>
        /// Everything needed to tell whether the client is in a state where scenarios can be handled
        /// at all. A null <see cref="HighLogic.CurrentGame"/> combined with an early network state
        /// means the send routine fired before the game was created, which only shows up on servers
        /// whose sync takes longer than the 30 second send interval.
        /// </summary>
        private static void AppendEnvironment(StringBuilder sb)
        {
            sb.AppendLine("--- Client state ---");
            AppendSafe(sb, "LMP version", () => LmpVersioning.CurrentVersion.ToString());
            AppendSafe(sb, "Seconds since scenario system enabled", SecondsSinceEnabled);
            AppendSafe(sb, "Network state", () => MainSystem.NetworkState.ToString());
            AppendSafe(sb, "Loaded scene", () => $"{HighLogic.LoadedScene} (isGame: {HighLogic.LoadedSceneIsGame})");
            AppendSafe(sb, "Unity thread", () => MainSystem.IsUnityThread.ToString());

            sb.AppendLine("--- Server ---");
            AppendSafe(sb, "Endpoint", () => NetworkMain.ClientConnection?.ServerConnection?.RemoteEndPoint?.ToString() ?? "(not connected)");
            AppendSafe(sb, "Connection status", () => NetworkMain.ClientConnection?.ConnectionStatus.ToString() ?? "(no client)");
            AppendSafe(sb, "Game mode", () => SettingsSystem.ServerSettings.GameMode.ToString());
            AppendSafe(sb, "Difficulty", () => SettingsSystem.ServerSettings.GameDifficulty.ToString());
            AppendSafe(sb, "Warp mode", () => SettingsSystem.ServerSettings.WarpMode.ToString());
            AppendSafe(sb, "Server parameters", () => SettingsSystem.ServerSettings.ServerParameters == null ? "NULL" : "present");
            AppendSafe(sb, "Transport", TransportSummary);

            sb.AppendLine("--- Game ---");
            AppendSafe(sb, "HighLogic.CurrentGame", () => HighLogic.CurrentGame == null ? "NULL" : "present");
            AppendSafe(sb, "CurrentGame.Mode", () => HighLogic.CurrentGame == null ? "(no game)" : HighLogic.CurrentGame.Mode.ToString());
            AppendSafe(sb, "CurrentGame.Parameters", () => HighLogic.CurrentGame == null ? "(no game)" : HighLogic.CurrentGame.Parameters == null ? "NULL" : "present");
            AppendSafe(sb, "CurrentGame.scenarios", () => HighLogic.CurrentGame?.scenarios == null ? "(none)" : HighLogic.CurrentGame.scenarios.Count.ToString());
            AppendSafe(sb, "ScenarioRunner.Instance", () => ScenarioRunner.Instance == null ? "NULL" : "present");
            AppendSafe(sb, "FlightGlobals.Bodies", () => FlightGlobals.Bodies == null ? "NULL" : FlightGlobals.Bodies.Count.ToString());
            AppendSafe(sb, "Serenity expansion", () => ExpansionsLoader.IsExpansionInstalled("Serenity").ToString());
            AppendSafe(sb, "MakingHistory expansion", () => ExpansionsLoader.IsExpansionInstalled("MakingHistory").ToString());
        }

        /// <summary>
        /// The scenario modules KSP currently has loaded, with the assembly each one comes from.
        /// This is the list the send routine walks, so the module that fails is always in here.
        /// </summary>
        private static void AppendLoadedModules(StringBuilder sb)
        {
            sb.AppendLine("--- Loaded scenario modules (ScenarioRunner) ---");
            try
            {
                var modules = ScenarioRunner.GetLoadedModules();
                if (modules == null)
                {
                    sb.AppendLine("  GetLoadedModules() returned null");
                    return;
                }

                var index = 0;
                foreach (var module in modules)
                {
                    if (module == null)
                    {
                        sb.AppendLine($"  [{index++}] (null entry)");
                        continue;
                    }

                    var moduleType = module.GetType();
                    sb.AppendLine($"  [{index++}] {moduleType.Name} " +
                                  $"(type: {moduleType.FullName}, assembly: {AssemblyNameOf(moduleType)}, " +
                                  $"ignoredForSend: {IgnoredScenarios.IgnoreSend.Contains(moduleType.Name)})");
                }

                if (index == 0)
                    sb.AppendLine("  (no scenario modules loaded)");
            }
            catch (Exception e)
            {
                sb.AppendLine($"  (enumerating loaded modules threw {e.GetType().Name}: {e.Message})");
            }
        }

        /// <summary>
        /// Every <see cref="ScenarioModule"/> subclass LMP discovered in the loaded assemblies. A
        /// scenario that is loaded in the game but missing from this list can never be synced, and a
        /// type coming from an unexpected assembly points at a mod specific to this install.
        /// </summary>
        private static void AppendScenarioTypeInventory(StringBuilder sb, IDictionary<string, Type> knownScenarioTypes)
        {
            sb.AppendLine("--- ScenarioModule types found in loaded assemblies ---");
            if (knownScenarioTypes == null)
            {
                sb.AppendLine("  (inventory not available)");
                return;
            }

            try
            {
                sb.AppendLine($"  Count: {knownScenarioTypes.Count}");
                foreach (var knownScenarioType in knownScenarioTypes)
                {
                    sb.AppendLine($"  {knownScenarioType.Key} " +
                                  $"(type: {knownScenarioType.Value?.FullName ?? "NULL"}, assembly: {AssemblyNameOf(knownScenarioType.Value)})");
                }
            }
            catch (Exception e)
            {
                sb.AppendLine($"  (enumerating scenario types threw {e.GetType().Name}: {e.Message})");
            }
        }

        #endregion

        #region Private helpers

        private static string AssemblyNameOf(Type type)
        {
            try
            {
                return type?.Assembly?.GetName()?.Name ?? "(unknown)";
            }
            catch (Exception e)
            {
                return $"(threw {e.GetType().Name})";
            }
        }

        /// <summary>
        /// Live Lidgren channel state. Packet/byte counters are compiled out of Release, but
        /// unsent/stored/withheld counts are read from the connection and tell us whether a
        /// fragmented scenario reply is sitting unacked or waiting on a missing fragment.
        /// </summary>
        private static string TransportSummary()
        {
            return Safe(() =>
            {
                var config = NetworkMain.Config;
                var connection = NetworkMain.ClientConnection?.ServerConnection;
                var stats = connection?.Statistics?.ToString()?.Replace("\r\n", " | ").Replace("\n", " | ").Trim(' ', '|');
                return $"mtuConfig={config.MaximumTransmissionUnit} autoExpand={config.AutoExpandMTU} " +
                       $"outgoingQueue={NetworkSender.OutgoingMessages.Count} pingMs={(NetworkStatistics.PingSec * 1000f):F0} " +
                       $"lidgren=[{stats ?? "no connection"}]";
            });
        }

        private static string SecondsSinceEnabled()
        {
            return _enabledAtUtc == DateTime.MinValue
                ? "(not enabled)"
                : $"{(DateTime.UtcNow - _enabledAtUtc).TotalSeconds:F1}";
        }

        /// <summary>
        /// Reads a value that touches KSP state, which may itself be in the broken state we are
        /// trying to describe. Never let the diagnostics throw over the original problem.
        /// </summary>
        private static string Safe(Func<string> valueProvider)
        {
            try
            {
                return valueProvider() ?? "(null)";
            }
            catch (Exception e)
            {
                return $"(threw {e.GetType().Name}: {e.Message})";
            }
        }

        private static void AppendSafe(StringBuilder sb, string label, Func<string> valueProvider)
        {
            sb.AppendLine($"  {label}: {Safe(valueProvider)}");
        }

        private static void Write(string title, Action<StringBuilder> body)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(Separator);
                sb.AppendLine($"{title} — {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}Z");
                body?.Invoke(sb);
                sb.AppendLine();

                var path = Path.Combine(MainSystem.KspPath, LogFileName);
                lock (FileLock)
                {
                    File.AppendAllText(path, sb.ToString(), Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Failed to write {LogFileName}: {e.Message}");
            }
        }

        #endregion
    }
}
