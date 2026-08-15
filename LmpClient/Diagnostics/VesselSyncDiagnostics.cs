using LmpClient.Systems.SettingsSys;
using LmpClient.Utilities;
using System;
using System.IO;
using System.Text;

namespace LmpClient.Diagnostics
{
    /// <summary>
    /// Per-session diagnostic trace of every vessel-proto wire event the client
    /// processes, written to <c>{KspPath}/Logs/LMP/VesselSyncLog.txt</c>. The
    /// file is truncated on every KSP launch — same lifecycle as KSP.log — so
    /// each file contains exactly one session's worth of events and the post-
    /// mortem grep target is unambiguous. Designed to answer four questions
    /// after the fact, without re-reading the entire KSP.log:
    /// <list type="number">
    /// <item>Which vessels did the server actually send me this session?</item>
    /// <item>Of those, which were rejected before they ever became a live
    /// <see cref="Vessel"/>, and exactly why (malformed config node, on the
    /// remove kill-list, failed Validate / HasInvalidParts, threw inside
    /// ProtoVessel.Load)?</item>
    /// <item>Of those that were accepted, which ones produced a brand-new
    /// <see cref="Vessel"/>, which were destructive reloads, which were
    /// cheap proto-swaps (SPACECENTER / EDITOR), and which were structural
    /// no-ops?</item>
    /// <item>What vessels do I currently "have"? (Implicit from the running
    /// sum of LOADED + RELOADED + SWAPPED minus kills.)</item>
    /// </list>
    ///
    /// Output is pipe-delimited so it is trivial to grep, awk, or paste into a
    /// spreadsheet. One line per event; the column layout is documented in the
    /// header banner written at the top of each new file. Pipes inside string
    /// fields are sanitised to '/' so the column count is invariant.
    ///
    /// Threading model: every <c>Log*</c> entry point is safe to call from any
    /// thread. The actual file write is serialised through a single lock; the
    /// writer is opened lazily on first successful call. We deliberately do NOT
    /// read <see cref="HighLogic.LoadedScene"/> from inside <c>Log*</c> because
    /// vessel-proto messages arrive on the LMP message-handling thread and KSP's
    /// Unity APIs are not guaranteed thread-safe; instead, the scene is cached
    /// by <see cref="NotifyScene"/> from the Unity-thread routine that owns the
    /// drain loop, and reads of that cache are <c>volatile</c> so a late wire
    /// event still records the most recent scene the player was actually in.
    ///
    /// Kill switch: gated on <see cref="SettingStructure.VesselSyncDiagnosticsEnabled"/>.
    /// Default-on because the I/O cost is negligible (vessel-proto wire events
    /// are seconds-scale, not per-frame), and the diagnostic value during the
    /// next bug report is significant. Flip the setting to false in settings.xml
    /// if it ever becomes noise. If file initialisation or a write itself ever
    /// throws, the diagnostic disables itself for the rest of the session and
    /// logs a single warning to KSP.log — diagnostic instrumentation must never
    /// break the load path it observes.
    /// </summary>
    public static class VesselSyncDiagnostics
    {
        private const string LogFolderName = "LMP";
        private const string LogFileName = "VesselSyncLog.txt";

        //One column separator. Pipes inside string fields are rewritten to '/'
        //by SanitiseField so column count is always exactly 8.
        private const string Sep = " | ";

        private static readonly object WriteLock = new object();
        private static StreamWriter _writer;

        //Hard kill once init or a write fails — we'd rather lose the diagnostic
        //than risk repeatedly logging warnings to KSP.log from a hot path.
        private static bool _disabled;

        //Cached scene id, written from Unity-thread routines (CheckVesselsToLoad)
        //and read from any thread. -1 means "we have not seen the Unity thread
        //yet this session", which is the case for the very first wire ARRIVED
        //log line before VesselProtoSystem's first Update tick.
        private static volatile int _lastKnownSceneId = -1;

        //Tracks "did Write() append a line since the last Flush()?" Read and written
        //under WriteLock so we never miss a write that landed between Flush()'s
        //dirty-check and clear. With AutoFlush=false the underlying StreamWriter
        //buffers writes in memory; this flag is what tells the periodic
        //Flush()-from-NotifyScene whether the syscall is actually worth issuing.
        //Without it an idle session would still issue ~41 no-op FileStream flushes
        //per second (one per CheckVesselsToLoad tick), which is the exact cost
        //AutoFlush=false was meant to avoid.
        private static bool _dirty;

        /// <summary>
        /// True iff the user has not turned the diagnostic off via settings.xml
        /// AND we have not auto-disabled after a write failure. Cheap enough to
        /// gate every log call on without short-circuiting at the call site.
        /// Defensive against an early-startup race where <c>SettingsSystem</c>'s
        /// static ctor has not yet completed: any throw on the settings read
        /// is interpreted as "off" so we never break a wire-message handler
        /// trying to ask whether we should record an event.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                if (_disabled) return false;
                try { return SettingsSystem.CurrentSettings.VesselSyncDiagnosticsEnabled; }
                catch { return false; }
            }
        }

        /// <summary>
        /// Caches the current loaded scene so log lines emitted from non-Unity
        /// threads (notably <c>VesselProtoMessageHandler.HandleMessage</c>) can
        /// still report a meaningful scene without touching <c>HighLogic</c>
        /// off-thread, AND opportunistically drains the writer's in-memory buffer
        /// to the OS. The drain is gated on <see cref="_dirty"/> so an idle
        /// session pays only one volatile-int store + one bool read per tick
        /// (~10 ns); a busy session collapses every Write since the last drain
        /// into a single <c>StreamWriter.Flush</c> syscall, instead of one
        /// syscall per Write the way <c>AutoFlush=true</c> would. Worst-case
        /// data loss on a hard process abort is bounded by the cadence of the
        /// caller (<c>CheckVesselsToLoad</c>, every Update tick — ≤24 ms at
        /// 41 fps); the trade is explicitly accepted for a debug log.
        /// </summary>
        public static void NotifyScene(GameScenes scene)
        {
            _lastKnownSceneId = (int)scene;
            FlushIfDirty();
        }

        /// <summary>
        /// Pushes any buffered writes to the OS. Safe to call from any thread; no-ops
        /// (cheaply) when nothing has been written since the last flush. Exposed
        /// publicly so <see cref="MainSystem.OnExit"/> can guarantee a drained
        /// trace on graceful quit — the FileStream finalizer would do this on
        /// AppDomain unload, but Unity's shutdown sequence does not always run
        /// finalizers and we'd rather not lose the last seconds of trace if a
        /// connection failure was the reason the user quit.
        /// </summary>
        public static void Flush()
        {
            FlushIfDirty();
        }

        private static void FlushIfDirty()
        {
            if (_disabled || !_dirty) return;
            try
            {
                lock (WriteLock)
                {
                    if (_disabled || !_dirty || _writer == null) return;
                    _writer.Flush();
                    _dirty = false;
                }
            }
            catch (Exception e)
            {
                //Same one-shot disable policy as Write(): never spam.
                _disabled = true;
                LunaLog.LogWarning($"[LMP]: VesselSyncDiagnostics flush failed; disabling for rest of session: {e.Message}");
            }
        }

        /// <summary>
        /// "Server sent us a VesselProto for this vessel id." Emitted exactly
        /// once per wire message, from the network thread, before any of the
        /// downstream filters (kill-list, malformed, validate, invalid parts,
        /// proto-swap eligibility, load) run. Anything that subsequently
        /// happens to this id within the next ~1 s is logged against the same
        /// id so the cause of a "I never saw this vessel" complaint can be
        /// reconstructed by grepping for the id.
        /// </summary>
        public static void LogArrived(Guid vesselId, int numBytes, double gameTime, bool forceReload, string reason)
        {
            if (!Enabled) return;
            var reasonText = string.IsNullOrEmpty(reason)
                ? $"bytes={numBytes} gameTime={gameTime:F2} forceReload={forceReload}"
                : $"bytes={numBytes} gameTime={gameTime:F2} forceReload={forceReload} senderReason={reason}";
            Write("ARRIVED", vesselId, vesselName: null, parts: -1, situation: null, reason: reasonText);
        }

        /// <summary>
        /// "This vessel will not become (or update) a live <see cref="Vessel"/>
        /// this tick because <paramref name="reason"/>." Logged from every
        /// reject site between the wire and a successful <c>ProtoVessel.Load</c>:
        /// <see cref="LmpClient.Systems.VesselProtoSys.VesselProtoMessageHandler"/>
        /// (kill-list), <see cref="LmpClient.Systems.VesselProtoSys.VesselProto.CreateProtoVessel"/>
        /// (malformed config node), and
        /// <see cref="LmpClient.Systems.VesselProtoSys.VesselProtoSystem.CheckVesselsToLoad"/>
        /// (kill-list re-check, <c>Validate</c> false, <c>HasInvalidParts</c>
        /// true, <c>UpdateProtoInPlace</c> exception fallback). One DISCARDED
        /// line per reject; a vessel that bounces between filters across
        /// multiple tick cycles will produce multiple DISCARDED lines (which is
        /// itself useful — recurring rejects mean the server is retransmitting
        /// a vessel we cannot accept).
        /// </summary>
        public static void LogDiscarded(Guid vesselId, string vesselName, int parts, string reason)
        {
            if (!Enabled) return;
            Write("DISCARDED", vesselId, vesselName, parts, situation: null, reason: reason);
        }

        /// <summary>
        /// "<c>VesselLoader.LoadVessel</c> returned <paramref name="outcome"/>."
        /// Emits a different event tag per outcome so the file is filterable
        /// by visible state: LOADED (brand-new live <see cref="Vessel"/>),
        /// RELOADED (destructive replace), UNCHANGED (early-out, the live
        /// Vessel already matched the wire structure), or FAILED (Load threw
        /// or returned a malformed orbit). LOADED + RELOADED + SWAPPED is the
        /// set of vessels the player can currently "see"; UNCHANGED and FAILED
        /// are present-but-no-op and reject-after-attempt respectively.
        /// </summary>
        public static void LogLoadOutcome(Guid vesselId, string vesselName, int parts, Vessel.Situations situation,
            LmpClient.VesselUtilities.VesselLoadOutcome outcome)
        {
            if (!Enabled) return;
            string tag;
            switch (outcome)
            {
                case LmpClient.VesselUtilities.VesselLoadOutcome.FreshlyLoaded: tag = "LOADED"; break;
                case LmpClient.VesselUtilities.VesselLoadOutcome.Reloaded: tag = "RELOADED"; break;
                case LmpClient.VesselUtilities.VesselLoadOutcome.UnchangedEarlyOut: tag = "UNCHANGED"; break;
                case LmpClient.VesselUtilities.VesselLoadOutcome.Failed: tag = "FAILED"; break;
                default: tag = outcome.ToString().ToUpperInvariant(); break;
            }
            Write(tag, vesselId, vesselName, parts, situation.ToString(), reason: null);
        }

        /// <summary>
        /// "<c>VesselLoader.UpdateProtoInPlace</c> succeeded — the live
        /// <see cref="Vessel"/> was not touched but its <c>protoVessel</c>
        /// pointer and the <c>flightState.protoVessels</c> entry now hold the
        /// fresh wire proto." Only emitted from the SPACECENTER / EDITOR fast
        /// path; the equivalent on the destructive path is RELOADED.
        /// </summary>
        public static void LogProtoSwapped(Guid vesselId, string vesselName, int parts, Vessel.Situations situation)
        {
            if (!Enabled) return;
            Write("SWAPPED", vesselId, vesselName, parts, situation.ToString(),
                reason: "lightweight-scene proto-swap (no destructive reload)");
        }

        /// <summary>
        /// "A live <see cref="Vessel"/> was removed via
        /// <c>VesselProtoSystem.RemoveVessel</c>." Provides a "we no longer
        /// have this vessel" event so the running set of "what the player can
        /// see" stays accurate without having to also tail KSP.log. <paramref
        /// name="reason"/> is the same string the call site passes to the
        /// server kill-vessel audit log when available.
        /// </summary>
        public static void LogRemoved(Guid vesselId, string vesselName, string reason)
        {
            if (!Enabled) return;
            Write("REMOVED", vesselId, vesselName, parts: -1, situation: null, reason: reason);
        }

        /// <summary>
        /// "The drain loop in <c>VesselProtoSystem.CheckVesselsToLoad</c> refused
        /// to apply an incoming proto for this vessel id this tick because a
        /// local topology mutation (Couple / Decouple / Undock) for that same id
        /// landed within the last
        /// <c>LocalTopologyTracker.QuarantineWindowSeconds</c>." Emitted at most
        /// once per quarantine episode (the tracker flips an internal flag the
        /// first time the drain observes the deferral). The matching follow-up
        /// event — LOADED / RELOADED / UNCHANGED / SWAPPED / DISCARDED — will
        /// appear on a subsequent tick once the cascade settles and the
        /// quarantine clears, so the post-mortem can confirm the deferred proto
        /// did eventually land. If a vessel produces DEFERRED with no follow-up
        /// in the same file, that means the quarantine outlived the queued
        /// proto's relevance (or the vessel was killed during the window).
        /// </summary>
        public static void LogDeferred(Guid vesselId, string vesselName, int parts, string reason)
        {
            if (!Enabled) return;
            Write("DEFERRED", vesselId, vesselName, parts, situation: null, reason: reason);
        }

        private static void Write(string ev, Guid vesselId, string vesselName, int parts, string situation, string reason)
        {
            if (_disabled) return;

            try
            {
                lock (WriteLock)
                {
                    if (_disabled) return;
                    var w = GetWriter();
                    if (w == null) return;

                    //Order MUST match the header banner written by GetWriter; an off-by-one
                    //in either direction makes the file unparseable for downstream tools.
                    var sb = new StringBuilder(160);
                    sb.Append(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
                    sb.Append(Sep).Append(ev);
                    sb.Append(Sep).Append(SceneTag());
                    sb.Append(Sep).Append(vesselId);
                    sb.Append(Sep).Append(SanitiseField(vesselName));
                    sb.Append(Sep).Append(parts >= 0 ? parts.ToString() : "?");
                    sb.Append(Sep).Append(SanitiseField(situation) ?? "?");
                    sb.Append(Sep).Append(SanitiseField(reason) ?? "");
                    w.WriteLine(sb.ToString());
                    //Set INSIDE the lock so a concurrent FlushIfDirty cannot read a
                    //stale false and skip a write that just landed. Writing the bool
                    //after the WriteLine is intentional — if WriteLine itself throws,
                    //we end up in the catch below and _disabled flips, so we never
                    //leave _dirty=true on a writer that's about to be unusable.
                    _dirty = true;
                }
            }
            catch (Exception e)
            {
                //One-shot: disable and warn. Do NOT keep trying — if the disk
                //is unwritable or we hit a sharing violation we don't want to
                //flood KSP.log with the same warning on every wire event.
                _disabled = true;
                LunaLog.LogWarning($"[LMP]: VesselSyncDiagnostics write failed; disabling for rest of session: {e.Message}");
            }
        }

        private static StreamWriter GetWriter()
        {
            if (_writer != null) return _writer;

            try
            {
                var folder = CommonUtil.CombinePaths(MainSystem.KspPath, "Logs", LogFolderName);
                Directory.CreateDirectory(folder);
                var path = CommonUtil.CombinePaths(folder, LogFileName);

                //FileMode.Create: truncate on every KSP launch so the file mirrors
                //KSP.log's per-session lifecycle. Anyone reporting a bug now has
                //a one-to-one mapping between their KSP.log and VesselSyncLog.txt
                //instead of having to find the right session boundary in a file
                //that grew across many launches.
                var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                //AutoFlush=false: every individual WriteLine would otherwise issue a
                //WriteFile syscall (~10–30 µs on SSD; multiple ms on a slow disk).
                //The periodic Flush() called from NotifyScene drains the buffer once
                //per Update tick, collapsing all writes since the last drain into a
                //single syscall. Per-event cost drops from ~25 µs to ~1–3 µs, and
                //on idle ticks the dirty-flag gate skips the syscall entirely.
                _writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = false };

                _writer.WriteLine("# Luna Multiplayer Vessel Sync Log");
                _writer.WriteLine("# Per-session: this file is truncated on every KSP launch (same lifecycle as KSP.log).");
                _writer.WriteLine("# Columns (pipe-delimited):");
                _writer.WriteLine("#   timestamp_utc | event | scene | vesselId | vesselName | parts | situation | reason");
                _writer.WriteLine("# Events:");
                _writer.WriteLine("#   ARRIVED   = wire message received from server (pre-filter)");
                _writer.WriteLine("#   DISCARDED = rejected before ProtoVessel.Load (kill-list / malformed / Validate / invalid parts)");
                _writer.WriteLine("#   LOADED    = brand-new live Vessel created");
                _writer.WriteLine("#   RELOADED  = existing Vessel destructively replaced");
                _writer.WriteLine("#   SWAPPED   = cheap proto-pointer swap (SPACECENTER / EDITOR; live Vessel untouched)");
                _writer.WriteLine("#   UNCHANGED = existing Vessel already matched the wire structure (no work done)");
                _writer.WriteLine("#   FAILED    = ProtoVessel.Load threw or produced a malformed orbit");
                _writer.WriteLine("#   REMOVED   = VesselProtoSystem.RemoveVessel ran (player can no longer 'see' it)");
                _writer.WriteLine("# To disable: set VesselSyncDiagnosticsEnabled=false in settings.xml.");
                _writer.WriteLine($"# session_start_utc={DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffZ} ksp_pid={CommonUtil.ProcessId}");

                //Banner is in-buffer only at this point — mark dirty so the next
                //NotifyScene-driven Flush pushes it to disk along with whatever
                //first event triggered this lazy init.
                _dirty = true;
                return _writer;
            }
            catch (Exception e)
            {
                _disabled = true;
                LunaLog.LogWarning($"[LMP]: VesselSyncDiagnostics init failed; disabling for rest of session: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resolves the scene tag for a log line. Reads the cached snapshot
        /// (written by <see cref="NotifyScene"/> from a Unity-thread routine)
        /// rather than touching <see cref="HighLogic.LoadedScene"/> directly,
        /// because <see cref="Write"/> can be invoked from the LMP network
        /// thread.
        /// </summary>
        private static string SceneTag()
        {
            var id = _lastKnownSceneId;
            if (id < 0) return "?";
            try { return ((GameScenes)id).ToString(); }
            catch { return "?"; }
        }

        /// <summary>
        /// Strips column separators and newlines from a free-form string so
        /// the pipe-delimited layout has invariant column count. Empty / null
        /// inputs round-trip as null (the caller decides whether to write
        /// "?" or "" in their place).
        /// </summary>
        private static string SanitiseField(string s)
        {
            if (s == null) return null;
            if (s.Length == 0) return s;
            //Cheap: only allocate a new string if we'd actually change something.
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '|' || c == '\r' || c == '\n')
                {
                    var sb = new StringBuilder(s.Length);
                    for (var j = 0; j < s.Length; j++)
                    {
                        var d = s[j];
                        sb.Append(d == '|' ? '/' : (d == '\r' || d == '\n') ? ' ' : d);
                    }
                    return sb.ToString();
                }
            }
            return s;
        }
    }
}
