using HarmonyLib;
using KSP.UI.Screens;
using LmpClient.Base;
using LmpClient.Diagnostics;
using LmpClient.Events;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace LmpClient.Systems.KscScene
{
    /// <summary>
    /// This class controls what happens when we are in KSC screen
    /// </summary>
    public class KscSceneSystem : System<KscSceneSystem>
    {
        private static MethodInfo BuildSpaceTrackingVesselList { get; } = typeof(SpaceTracking).GetMethod("buildVesselsList", AccessTools.all);

        private static KscSceneEvents KscSceneEvents { get; } = new KscSceneEvents();

        //Coalescing flag for the tracking-station vessel list rebuild. All four
        //event handlers in KscSceneEvents (OnLockAcquire / OnLockRelease /
        //OnVesselCreated / VesselInitialized) used to call
        //RefreshTrackingStationVessels() synchronously, which reflection-invokes
        //stock SpaceTracking.buildVesselsList — a full O(N) widget teardown +
        //rebuild over every vessel in FlightGlobals.Vessels. Stock KSP fires
        //GameEvents.onVesselCreate once per ProtoVessel during TRACKSTATION
        //scene init, so on a heavily populated server (e.g. 114 vessels) that
        //produced 114 back-to-back full rebuilds in the first frame of TS,
        //≈9.1s of pure synchronous work on the Unity main thread (confirmed
        //via the TsLoadProfiler trace: RefreshTS=114 (9110.0ms)
        //OnVesselCreated=114 (9110.1ms) — the OnVesselCreated bucket double-
        //counts the same time because it wraps the inner call). The fix:
        //requesters set this flag, and a single LateUpdate routine drains it
        //into one rebuild per frame regardless of how many handlers fired.
        //
        //Read/written exclusively from the Unity main thread (LockSystem is a
        //MessageSystem with the default ProcessMessagesInUnityThread = true,
        //so onLockAcquire/onLockRelease arrive here on the Unity thread; KSP
        //GameEvents always fire on the Unity thread). No interlock needed; the
        //field is volatile only as a documentation hint that it's a cross-
        //frame signaling primitive.
        private static volatile bool _trackingStationRebuildPending;

        #region Base overrides

        public override string SystemName { get; } = nameof(KscSceneSystem);

        protected override void OnEnabled()
        {
            LockEvent.onLockAcquire.Add(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockRelease.Add(KscSceneEvents.OnLockRelease);
            GameEvents.onGameSceneLoadRequested.Add(KscSceneEvents.OnSceneRequested);
            GameEvents.onLevelWasLoadedGUIReady.Add(KscSceneEvents.LevelLoaded);
            GameEvents.onVesselCreate.Add(KscSceneEvents.OnVesselCreated);
            VesselInitializeEvent.onVesselInitialized.Add(KscSceneEvents.VesselInitialized);
            GameEvents.onVesselRename.Add(KscSceneEvents.OnVesselRename);

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.FixedUpdate, IncreaseTimeWhileInEditor));
            //Drain the coalesced TS rebuild request at most once per frame.
            //LateUpdate (rather than Update) so all event handlers in the same
            //frame that mark the flag — including OnVesselCreated bursts that
            //KSP fires synchronously during TS scene initialization — get
            //folded into a single rebuild rather than two (one for any flags
            //set during Update, one for OnVesselCreated bursts that fire
            //during scene activation immediately after Update).
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.LateUpdate, FlushPendingRefreshes));
        }

        protected override void OnDisabled()
        {
            LockEvent.onLockAcquire.Remove(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockRelease.Remove(KscSceneEvents.OnLockRelease);
            GameEvents.onGameSceneLoadRequested.Remove(KscSceneEvents.OnSceneRequested);
            GameEvents.onLevelWasLoadedGUIReady.Remove(KscSceneEvents.LevelLoaded);
            GameEvents.onVesselCreate.Remove(KscSceneEvents.OnVesselCreated);
            VesselInitializeEvent.onVesselInitialized.Remove(KscSceneEvents.VesselInitialized);
            GameEvents.onVesselRename.Remove(KscSceneEvents.OnVesselRename);
        }

        #endregion

        #region Routines

        /// <summary>
        /// While in editor the time doesn't advance so here we make it advance
        /// </summary>
        private static void IncreaseTimeWhileInEditor()
        {
            if (!HighLogic.LoadedSceneHasPlanetarium && HighLogic.LoadedScene >= GameScenes.SPACECENTER)
            {
                Planetarium.fetch.time += Time.fixedDeltaTime;
                HighLogic.CurrentGame.flightState.universalTime = Planetarium.fetch.time;
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Requests a refresh of the vessels displayed in the tracking station
        /// panel. Coalesced: multiple calls in the same frame (and across
        /// adjacent Update/LateUpdate of the same frame) collapse to a single
        /// stock <c>SpaceTracking.buildVesselsList</c> invocation drained by
        /// <see cref="FlushPendingRefreshes"/>. See the comment on
        /// <see cref="_trackingStationRebuildPending"/> for the full rationale
        /// and the trace numbers that motivated the debounce.
        ///
        /// Public signature is unchanged from before the debounce so all four
        /// existing callers in <see cref="KscSceneEvents"/> keep working with
        /// no changes; the per-call cost is now O(1) (a flag write plus the
        /// existing diagnostic timing) instead of O(N) per vessel.
        /// </summary>
        public void RefreshTrackingStationVessels()
        {
            //The existing per-event bucket continues to count call frequency;
            //after the debounce its elapsed-ms column should report ~0 for the
            //entire post-fix lifetime, which is itself a useful invariant: any
            //future regression that puts heavy work back on this hot path will
            //immediately show up in the per-second profile line.
            var t0 = Stopwatch.GetTimestamp();
            _trackingStationRebuildPending = true;
            TsLoadProfiler.Record(TsLoadProfiler.Bucket.RefreshTrackingStationVessels, Stopwatch.GetTimestamp() - t0);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// LateUpdate routine that drains <see cref="_trackingStationRebuildPending"/>
        /// at most once per frame. Runs unconditionally — the actual rebuild
        /// in <see cref="DoRefreshTrackingStationVessels"/> is gated on the
        /// scene check, so a flag left set across a SPACECENTER → TRACKSTATION
        /// transition correctly produces one refresh once we're in the new
        /// scene, and a flag set on the last frame of a TRACKSTATION session
        /// silently no-ops (the UI is already torn down by then anyway).
        /// </summary>
        private static void FlushPendingRefreshes()
        {
            if (!_trackingStationRebuildPending) return;
            _trackingStationRebuildPending = false;
            DoRefreshTrackingStationVessels();
        }

        /// <summary>
        /// The original synchronous body of <see cref="RefreshTrackingStationVessels"/>:
        /// scene-graph scan for the active <see cref="SpaceTracking"/> instance and a
        /// reflection invoke into stock KSP's private
        /// <c>SpaceTracking.buildVesselsList</c>, which tears down and rebuilds every
        /// tracking-station widget from <see cref="FlightGlobals.Vessels"/>. This is
        /// genuinely O(vessels) per call and is the dominant cost the debounce exists
        /// to coalesce — recorded under its own profiler bucket so we can see how much
        /// real work survives the debounce (typically 1 call per frame regardless of
        /// how many requesters fired into it that frame).
        /// </summary>
        private static void DoRefreshTrackingStationVessels()
        {
            var t0 = Stopwatch.GetTimestamp();
            if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
            {
                var spaceTracking = Object.FindObjectOfType<SpaceTracking>();
                if (spaceTracking != null)
                    BuildSpaceTrackingVesselList?.Invoke(spaceTracking, null);
            }
            TsLoadProfiler.Record(TsLoadProfiler.Bucket.TsRebuildFlush, Stopwatch.GetTimestamp() - t0);
        }

        #endregion
    }
}
