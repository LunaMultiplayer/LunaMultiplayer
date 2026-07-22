using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LmpClient.Systems.VesselProtoSys
{
    /// <summary>
    /// Per-vessel record of "we just rewrote this vessel's part tree locally, so
    /// don't apply an incoming server proto for it for a brief settling window."
    ///
    /// Background. The grapple/dock/decouple pipeline produces a burst of part-
    /// tree mutations within a single physics frame (one stock <c>Part.Couple</c>
    /// followed by N stock <c>Part.decouple</c> events when Breaking Ground
    /// robotic joints or stock docking-port stacks pop apart from the coupling
    /// impulse). Each LMP-patched mutation broadcasts a vessel definition. When a
    /// server-sourced VesselProto for the *same* vessel id was already in flight
    /// at the moment of the local mutation (e.g. another player or the periodic
    /// drift broadcast), the receiving-side
    /// <see cref="VesselProtoSystem.CheckVesselsToLoad"/> drain will happily call
    /// <c>ProtoVessel.Load</c> against a vessel the local engine has just
    /// rebuilt — replacing the freshly-coupled tree with a stale snapshot, which
    /// in turn re-fires stock decouple events, recurses into the same broadcast
    /// path, and in pathological cases leaves the focus vessel with garbage
    /// transforms / velocities that materialise as a "Krakened" hyperbolic exit
    /// (the original KSP.log evidence shows ~82 km/s ejection out of Mun + a
    /// rolling sequence of "crashed through terrain" kills on every other
    /// fragment of the affected station).
    ///
    /// This tracker is the receiving-side counterpart to the existing outgoing
    /// <see cref="VesselProtoSystem.DelayedSendVesselMessage"/>: just as the
    /// outgoing path coalesces post-decouple broadcasts over a 500 ms window,
    /// the incoming path now waits a short
    /// <see cref="QuarantineWindowSeconds"/> after the *last* local topology
    /// mutation for that vessel id before applying any wire update. Each new
    /// mutation resets the clock, so a sustained cascade of robotic-joint /
    /// docking-port breaks extends the quarantine for as long as the part tree
    /// is actually still moving — which is the property we want.
    ///
    /// Threading. Recording happens from KSP/Harmony-fired
    /// <see cref="LmpClient.Events.PartEvent"/> handlers (Unity main thread).
    /// Querying happens from <c>CheckVesselsToLoad</c> (Unity main thread).
    /// <c>RemoveVessel</c> can be invoked from message-handling threads, so the
    /// backing store is a <see cref="ConcurrentDictionary{TKey,TValue}"/>. We
    /// read <see cref="Time.realtimeSinceStartup"/> (wall-clock seconds since
    /// process start, monotonically increasing, unaffected by KSP time-warp or
    /// pause) as the clock source. This matches the convention already used
    /// elsewhere in the client (<c>OrbitDriver_UpdateFromParameters</c>,
    /// <c>SpectateDebug</c>); the alternative <c>Time.unscaledTimeAsDouble</c>
    /// would be cleaner but is a Unity 2020.2+ API and KSP 1.12 still ships
    /// the Unity 2019.4 runtime.
    ///
    /// Design notes that didn't end up in the code itself:
    /// 1. We deliberately key by <see cref="Guid"/> (vessel id), not by
    ///    <see cref="Vessel"/> reference, because mutations frequently destroy
    ///    the very <see cref="Vessel"/> instance that started the cascade (the
    ///    "weak" vessel of a couple, or any decouple that empties the original
    ///    part collection). Guid persists across the destroy.
    /// 2. The DeferralLogged flag is included so we only emit ONE
    ///    <c>VesselSyncDiagnostics.LogDeferred</c> line per quarantine episode,
    ///    rather than one per <c>CheckVesselsToLoad</c> tick (~41 Hz). The
    ///    successful drain at the end of the episode pairs naturally with the
    ///    subsequent LOADED / RELOADED / SWAPPED / UNCHANGED line in the
    ///    diagnostic file so the cause-effect is reconstructable post-hoc.
    /// 3. We do NOT cap the dictionary size or evict entries on a timer because
    ///    the natural cleanup path is <see cref="ClearVessel"/> driven by
    ///    <c>VesselProtoSystem.RemoveVessel</c>. A vessel that mutates once and
    ///    is never seen again still leaves a single 24-byte entry behind for
    ///    the life of the session; that's an acceptable steady-state cost
    ///    versus the complexity of an eviction sweep.
    /// </summary>
    internal static class LocalTopologyTracker
    {
        /// <summary>
        /// How long after the last local mutation we refuse to apply incoming
        /// protos. Chosen by inspection of the original incident KSP.log: the
        /// full Couple → cascade-of-Decouples window from grab to scene-switch
        /// was ~370 ms. Per-event re-arming inside <see cref="RecordMutation"/>
        /// means a sustained cascade naturally extends past this value, so the
        /// window only needs to be long enough to bridge the inter-event gaps
        /// (sub-millisecond at 50 Hz physics). 250 ms is comfortably above that
        /// while still being short enough that legitimate proto updates aren't
        /// noticeably delayed when the player is just sitting still in flight.
        /// </summary>
        private const float QuarantineWindowSeconds = 0.25f;

        private readonly struct MutationRecord
        {
            public readonly float LastMutationRealtime;
            public readonly bool DeferralLogged;

            public MutationRecord(float t, bool logged)
            {
                LastMutationRealtime = t;
                DeferralLogged = logged;
            }
        }

        private static readonly ConcurrentDictionary<Guid, MutationRecord> Records =
            new ConcurrentDictionary<Guid, MutationRecord>();

        /// <summary>
        /// Record that a local topology mutation just touched <paramref name="vesselId"/>.
        /// Resets the quarantine clock and clears the per-episode "we already
        /// logged a deferral for this vessel" flag so that the next deferral
        /// emits a fresh line in <see cref="LmpClient.Diagnostics.VesselSyncDiagnostics"/>.
        /// Safe to call with <see cref="Guid.Empty"/>; the call is a no-op.
        /// </summary>
        public static void RecordMutation(Guid vesselId)
        {
            if (vesselId == Guid.Empty) return;
            var record = new MutationRecord(Time.realtimeSinceStartup, logged: false);
            Records[vesselId] = record;
        }

        /// <summary>
        /// Returns true if <paramref name="vesselId"/> had a topology mutation
        /// within the last <see cref="QuarantineWindowSeconds"/>. The out
        /// <paramref name="firstObservation"/> is true iff this is the first
        /// call observing the current quarantine episode (the flag flips inside
        /// the dictionary atomically) — caller uses it to gate the one-shot
        /// "DEFERRED" diagnostic line so we don't write one per frame.
        /// </summary>
        public static bool IsQuarantined(Guid vesselId, out bool firstObservation)
        {
            firstObservation = false;
            if (vesselId == Guid.Empty) return false;
            if (!Records.TryGetValue(vesselId, out var record)) return false;

            var now = Time.realtimeSinceStartup;
            if (now - record.LastMutationRealtime > QuarantineWindowSeconds)
                return false;

            if (!record.DeferralLogged)
            {
                //AddOrUpdate so we only set the logged flag on the entry that's
                //actually in the dictionary right now — if a concurrent
                //RecordMutation has overwritten the entry between our TryGetValue
                //and here, we want the logged flag flipped on the *new* entry
                //(its own DeferralLogged would also be false, so behaviour is
                //consistent). update factory ignores oldRec.DeferralLogged for
                //the same reason — we always want the post-call state to be
                //"logged".
                Records.AddOrUpdate(vesselId,
                    addValueFactory: _ => new MutationRecord(record.LastMutationRealtime, logged: true),
                    updateValueFactory: (_, oldRec) => new MutationRecord(oldRec.LastMutationRealtime, logged: true));
                firstObservation = true;
            }

            return true;
        }

        /// <summary>
        /// Drop the tracker entry for <paramref name="vesselId"/>. Invoked from
        /// <see cref="VesselProtoSystem.RemoveVessel"/> so a vessel that gets
        /// killed and respawned with the same id in the same session starts
        /// from a clean slate.
        /// </summary>
        public static void ClearVessel(Guid vesselId)
        {
            if (vesselId == Guid.Empty) return;
            Records.TryRemove(vesselId, out _);
        }

        /// <summary>
        /// Drop everything. Called from <see cref="VesselProtoSystem.OnDisabled"/>
        /// when the LMP client is shutting down or returning to the main menu.
        /// </summary>
        public static void ClearAll() => Records.Clear();
    }
}
