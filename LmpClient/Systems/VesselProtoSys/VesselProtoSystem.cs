using LmpClient.Base;
using LmpClient.Diagnostics;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.Systems.KerbalSys;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Utilities;
using LmpClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LmpClient.Systems.VesselProtoSys
{
    /// <summary>
    /// This system handles the vessel loading into the game and sending our vessel structure to other players.
    /// </summary>
    public class VesselProtoSystem : MessageSystem<VesselProtoSystem, VesselProtoMessageSender, VesselProtoMessageHandler>
    {
        #region Fields & properties

        private static readonly HashSet<Guid> QueuedVesselsToSend = new HashSet<Guid>();

        public readonly HashSet<Guid> VesselsUnableToLoad = new HashSet<Guid>();

        public ConcurrentDictionary<Guid, VesselProtoQueue> VesselProtos { get; } = new ConcurrentDictionary<Guid, VesselProtoQueue>();

        //Per-vessel record of "the value of vessel.parts.Count the last time we
        //broadcast a Part-count-drift message for this vessel id". Gates
        //SendVesselDefinition's drift check so we never re-broadcast the same drift
        //state more than once. Background: SendVesselDefinition fires every 2.5 s and
        //compares vessel.parts.Count against vessel.protoVessel.protoPartSnapshots.Count.
        //SendVesselMessage rewrites vessel.protoVessel via vessel.BackupVessel() on
        //every send, BUT stock KSP's BackupVessel can legitimately produce a snapshot
        //count that differs from the live parts.Count for several reasons (dynamic
        //parts, EVA-suit attachment, robotic / breaking-parts states that don't
        //round-trip 1:1 through serialisation). When that happens the drift check
        //fires on every 2.5 s tick forever, broadcasting an identical proto and
        //forcing every receiving client to pay a destructive ProtoVessel.Load on each
        //arrival -- exactly the ~3 s cadence of full reloads observed in KSP.log when
        //multiple peer vessels are present. Caching the count we last broadcast lets
        //us short-circuit when the drift is "stable" (server has the latest data;
        //nothing new to send) while still firing immediately when parts.Count moves
        //to a value we have not yet sent (genuine structural change on the
        //originating side).
        private static readonly ConcurrentDictionary<Guid, int> LastBroadcastDriftPartCount =
            new ConcurrentDictionary<Guid, int>();

        //Maximum number of expensive ProtoVessel.Load calls (fresh load or destructive
        //reload) we will execute in a single CheckVesselsToLoad tick. The drain loop
        //previously processed every queue whose head was ready in the same frame, so a
        //burst of N peer-side broadcasts produced an N x ProtoVessel.Load spike --
        //visible as 200-1000 ms of unaccounted single-frame work in KSP.log when three
        //or more vessels needed reloading simultaneously, exactly the "1 second of lag
        //every few seconds in the VAB" pattern. Capping the per-frame budget spreads
        //the same total work across multiple frames; queues whose heads we skip stay
        //peeked (not dequeued) so they are retried next tick in FIFO order with no
        //starvation. Cheap proto-swap operations in SPACECENTER / EDITOR do NOT count
        //against this budget -- only stock-KSP ProtoVessel.Load calls do, because they
        //are the actual cost driver. 2 is conservative: enough headroom to catch up
        //after a brief network burst without letting any single frame eat two full
        //destructive reloads back-to-back from a dead start.
        private const int MaxExpensiveReloadsPerTick = 2;

        public bool ProtoSystemReady => Enabled && FlightGlobals.ready && HighLogic.LoadedScene == GameScenes.FLIGHT &&
            FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating;

        public VesselProtoEvents VesselProtoEvents { get; } = new VesselProtoEvents();

        public VesselRemoveSystem VesselRemoveSystem => VesselRemoveSystem.Singleton;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselProtoSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            GameEvents.onFlightReady.Add(VesselProtoEvents.FlightReady);
            GameEvents.onGameSceneLoadRequested.Add(VesselProtoEvents.OnSceneRequested);

            GameEvents.OnTriggeredDataTransmission.Add(VesselProtoEvents.TriggeredDataTransmission);
            GameEvents.OnExperimentStored.Add(VesselProtoEvents.ExperimentStored);
            ExperimentEvent.onExperimentReset.Add(VesselProtoEvents.ExperimentReset);

            PartEvent.onPartDecoupled.Add(VesselProtoEvents.PartDecoupled);
            PartEvent.onPartUndocked.Add(VesselProtoEvents.PartUndocked);
            PartEvent.onPartCoupled.Add(VesselProtoEvents.PartCoupled);

            WarpEvent.onTimeWarpStopped.Add(VesselProtoEvents.WarpStopped);

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, SendVesselDefinition));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            GameEvents.onFlightReady.Remove(VesselProtoEvents.FlightReady);
            GameEvents.onGameSceneLoadRequested.Remove(VesselProtoEvents.OnSceneRequested);

            GameEvents.OnTriggeredDataTransmission.Remove(VesselProtoEvents.TriggeredDataTransmission);
            GameEvents.OnExperimentStored.Remove(VesselProtoEvents.ExperimentStored);
            ExperimentEvent.onExperimentReset.Remove(VesselProtoEvents.ExperimentReset);

            PartEvent.onPartDecoupled.Remove(VesselProtoEvents.PartDecoupled);
            PartEvent.onPartUndocked.Remove(VesselProtoEvents.PartUndocked);
            PartEvent.onPartCoupled.Remove(VesselProtoEvents.PartCoupled);

            WarpEvent.onTimeWarpStopped.Remove(VesselProtoEvents.WarpStopped);

            //This is the main system that handles the vesselstore so if it's disabled clear the store too
            VesselProtos.Clear();
            VesselsUnableToLoad.Clear();
            QueuedVesselsToSend.Clear();
            LastBroadcastDriftPartCount.Clear();
            LocalTopologyTracker.ClearAll();
        }

        #endregion

        #region Update routines

        /// <summary>
        /// Send the definition of our own vessel and the secondary vessels.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (!ProtoSystemReady) return;

                if (ShouldBroadcastDriftFor(FlightGlobals.ActiveVessel))
                    MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, reason: "Part count drift (active vessel)");

                foreach (var vessel in VesselCommon.GetSecondaryVessels())
                {
                    if (ShouldBroadcastDriftFor(vessel))
                        MessageSender.SendVesselMessage(vessel, reason: "Part count drift (secondary vessel)");
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in SendVesselDefinition {e}");
            }

        }

        /// <summary>
        /// Returns true when <paramref name="vessel"/> has a part-count drift we have not
        /// already broadcast. Combines two short-circuits:
        /// 1. <c>parts.Count == protoVessel.protoPartSnapshots.Count</c> means the live
        ///    Vessel and its stored proto agree -- no drift, nothing to send.
        /// 2. <c>parts.Count == LastBroadcastDriftPartCount[vesselId]</c> means we already
        ///    broadcast at this exact part count; the server has the latest data and
        ///    re-sending would just trigger an identical receiving-side reload storm.
        /// The cache entry is updated when (and only when) we decide a broadcast is
        /// warranted, so any subsequent change to <c>parts.Count</c> on the originating
        /// side immediately re-arms the check.
        /// </summary>
        private static bool ShouldBroadcastDriftFor(Vessel vessel)
        {
            if (vessel == null || vessel.protoVessel?.protoPartSnapshots == null) return false;

            var liveCount = vessel.parts.Count;
            var protoCount = vessel.protoVessel.protoPartSnapshots.Count;
            if (liveCount == protoCount) return false;

            //ConcurrentDictionary even though VesselProtoSystem is single-threaded for
            //the send path: we also clear entries from RemoveVessel which can be
            //invoked from message-handling threads. The cost difference vs Dictionary
            //is irrelevant at 2.5 s tick granularity.
            if (LastBroadcastDriftPartCount.TryGetValue(vessel.id, out var lastSent) && lastSent == liveCount)
                return false;

            LastBroadcastDriftPartCount[vessel.id] = liveCount;
            return true;
        }

        /// <summary>
        /// Returns true for scenes where peer vessels exist in FlightGlobals strictly as
        /// data carriers (unloaded / packed) and the player cannot see or interact with
        /// them in-world. In those scenes a wire-side structural update can be applied
        /// with a cheap proto-swap (<see cref="VesselLoader.UpdateProtoInPlace"/>) instead
        /// of the full destructive <see cref="VesselLoader.LoadVessel"/> path; the latter
        /// would still create a brand-new <see cref="Vessel"/> <see cref="UnityEngine.GameObject"/>,
        /// run every <c>VesselModule.Awake</c>, and pay stock KSP's per-part persistentId
        /// collision walk for no visible benefit while the player is in the VAB / SPH or
        /// the Space Center scene.
        /// In FLIGHT (the in-world vessel may be loaded and rendered) and TRACKSTATION
        /// (the UI binds against the live <see cref="Vessel"/>'s vesselModules / crew
        /// portraits) we keep the destructive path so the player-visible state stays in
        /// lockstep with the wire.
        /// </summary>
        private static bool IsProtoSwapEligibleScene(GameScenes scene)
            => scene == GameScenes.SPACECENTER || scene == GameScenes.EDITOR;

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        public void CheckVesselsToLoad()
        {
            if (HighLogic.LoadedScene < GameScenes.SPACECENTER) return;

            //Snapshot the current scene for the diagnostic writer so the next
            //batch of network-thread ARRIVED / DISCARDED lines can record a
            //meaningful scene without touching HighLogic off-thread. Cheap (one
            //int store, no allocation) and only runs when the player is in a
            //scene that actually processes wire vessel updates.
            VesselSyncDiagnostics.NotifyScene(HighLogic.LoadedScene);

            try
            {
                // Drain any KerbalProto messages queued since the last KerbalSystem.LoadKerbals
                // tick BEFORE running vesselProto.Load(...) on any vessel this tick. Background:
                // KerbalSystem.LoadKerbals runs on its own ~1 s routine and consumes
                // KerbalsToProcess only during that tick; CheckVesselsToLoad runs on a faster
                // routine, so on a busy session (or during the burst that immediately follows
                // initial sync, when KerbalProto messages can still be in flight as VesselProto
                // messages start arriving) we can land here with named-but-unmerged kerbals
                // sitting in KerbalsToProcess. Stock KSP's ProtoPartSnapshot ConfigNode ctor
                // resolves "crew = NAME" against HighLogic.CurrentGame.CrewRoster *as it stands
                // right now*, so any kerbal that's queued-but-not-merged still produces a null
                // entry in protoModuleCrew + the "[Protocrewmember]: Instance of crewmember  in
                // part X on Y does not exist in the roster" warning, exactly as if the kerbal
                // had never been sent. Co-sending crew with each VesselProto already eliminates
                // the originating-side gap; this drain eliminates the receiving-side timing
                // gap so the two together close the loop. Cheap on the steady state -- the
                // queue is normally empty, this is one TryDequeue/Count check.
                if (KerbalSystem.Singleton != null && !KerbalSystem.Singleton.KerbalsToProcess.IsEmpty)
                {
                    KerbalSystem.Singleton.LoadKerbalsIntoGame();
                }

                var protoSwapEligible = IsProtoSwapEligibleScene(HighLogic.LoadedScene);
                var expensiveReloadsRemaining = MaxExpensiveReloadsPerTick;

                foreach (var keyVal in VesselProtos)
                {
                    if (!keyVal.Value.TryPeek(out var vesselProto)) continue;
                    if (vesselProto.GameTime > TimeSyncSystem.UniversalTime) continue;

                    //Topology-mutation quarantine. If we just locally rewrote this
                    //vessel's part tree via Couple / Decouple / Undock, refuse to
                    //apply incoming server protos for it until the cascade has
                    //settled (~250 ms after the last local mutation, with each
                    //new mutation re-arming the clock). Leaving the queue head
                    //peeked-but-not-dequeued retries it on the next Update tick
                    //in FIFO order so there is no risk of starvation. LogDeferred
                    //is one-shot per episode (see LocalTopologyTracker for the
                    //flag mechanics); the matching follow-up event will appear in
                    //the diagnostic file when the quarantine clears.
                    if (LocalTopologyTracker.IsQuarantined(keyVal.Key, out var firstObservation))
                    {
                        if (firstObservation)
                        {
                            VesselSyncDiagnostics.LogDeferred(keyVal.Key, vesselName: null, parts: -1,
                                reason: "LocalTopologyTracker quarantine (Couple/Decouple/Undock within last 250 ms)");
                        }
                        continue;
                    }

                    //Probe FlightGlobals BEFORE dequeuing so we can decide whether this
                    //tick will be cheap (proto-swap) or expensive (ProtoVessel.Load) and
                    //rate-limit only the latter. When we're over budget on the
                    //expensive path we leave the head peeked-but-not-dequeued so the
                    //very next CheckVesselsToLoad tick retries it in FIFO order -- there
                    //is no risk of starvation because TryPeek is non-mutating.
                    var existingVessel = FlightGlobals.FindVessel(vesselProto.VesselId);
                    var willUseProtoSwap = protoSwapEligible && existingVessel != null && !vesselProto.ForceReload;

                    if (!willUseProtoSwap && expensiveReloadsRemaining <= 0)
                        continue;

                    keyVal.Value.TryDequeue(out _);

                    if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.VesselId))
                    {
                        VesselSyncDiagnostics.LogDiscarded(vesselProto.VesselId, vesselName: null, parts: -1,
                            reason: "VesselRemoveSystem.VesselWillBeKilled returned true on drain (race vs network thread)");
                        //Recycle on the kill-list path too, otherwise the proto buffer
                        //leaks back to the pool only on the success branches below.
                        keyVal.Value.Recycle(vesselProto);
                        continue;
                    }

                    var forceReload = vesselProto.ForceReload;
                    var protoVessel = vesselProto.CreateProtoVessel();
                    var vesselId = vesselProto.VesselId;
                    keyVal.Value.Recycle(vesselProto);

                    var verboseErrors = !VesselsUnableToLoad.Contains(vesselId);
                    if (protoVessel == null)
                    {
                        //CreateProtoVessel already logged its own DISCARDED line with
                        //the precise malformed-config-node reason, so we don't double-
                        //log here -- just bookkeep the kill-list and move on.
                        VesselsUnableToLoad.Add(vesselId);
                        continue;
                    }
                    if (!protoVessel.Validate(verboseErrors))
                    {
                        VesselSyncDiagnostics.LogDiscarded(vesselId, SafeName(protoVessel),
                            SafePartCount(protoVessel),
                            reason: "ProtoVessel.Validate returned false");
                        VesselsUnableToLoad.Add(vesselId);
                        continue;
                    }
                    if (protoVessel.HasInvalidParts(verboseErrors))
                    {
                        VesselSyncDiagnostics.LogDiscarded(vesselId, SafeName(protoVessel),
                            SafePartCount(protoVessel),
                            reason: "ProtoVessel.HasInvalidParts returned true (one or more part definitions absent from local install)");
                        VesselsUnableToLoad.Add(vesselId);
                        continue;
                    }

                    VesselsUnableToLoad.Remove(vesselId);

                    if (willUseProtoSwap)
                    {
                        //SPACECENTER / EDITOR fast path: pointer-swap protoVessel without
                        //destroying the live Vessel. No reload event fires here -- the
                        //live Vessel was never touched, so any listener that did real
                        //work on reload would be running on a no-op. The flightState
                        //ProtoVessel list (the source-of-truth for save / scene
                        //transition) is updated to the fresh proto. If the in-place
                        //swap itself fails for any reason, fall through to the
                        //destructive path so we don't silently drop a wire update.
                        if (VesselLoader.UpdateProtoInPlace(existingVessel, protoVessel))
                        {
                            VesselSyncDiagnostics.LogProtoSwapped(vesselId, SafeName(protoVessel),
                                SafePartCount(protoVessel), SafeSituation(protoVessel));
                            continue;
                        }
                        VesselSyncDiagnostics.LogDiscarded(vesselId, SafeName(protoVessel),
                            SafePartCount(protoVessel),
                            reason: "UpdateProtoInPlace returned false (falling through to destructive reload)");
                    }

                    //Expensive path -- count it against the per-tick budget. We
                    //decrement even on Failed / UnchangedEarlyOut because the cost of
                    //ProtoVessel.Load was already paid by the time we know which
                    //outcome we got, and over-budget-failures are still wall-clock
                    //expensive.
                    expensiveReloadsRemaining--;

                    var outcome = VesselLoader.LoadVessel(protoVessel, forceReload);
                    VesselSyncDiagnostics.LogLoadOutcome(vesselId, SafeName(protoVessel),
                        SafePartCount(protoVessel), SafeSituation(protoVessel), outcome);
                    switch (outcome)
                    {
                        case VesselLoadOutcome.FreshlyLoaded:
                            LunaLog.Log($"[LMP]: Vessel {protoVessel.vesselID} loaded");
                            VesselLoadEvent.onLmpVesselLoaded.Fire(protoVessel.vesselRef);
                            break;
                        case VesselLoadOutcome.Reloaded:
                            LunaLog.Log($"[LMP]: Vessel {protoVessel.vesselID} reloaded");
                            VesselReloadEvent.onLmpVesselReloaded.Fire(protoVessel.vesselRef);
                            break;
                        case VesselLoadOutcome.UnchangedEarlyOut:
                            //Stock-matched early-out from LoadVesselIntoGame. Deliberately
                            //silent in KSP.log: no "reloaded" line and no VesselReloadEvent
                            //fire because nothing actually changed. The sync diagnostic
                            //still gets the UNCHANGED event above because "incoming wire
                            //update for an already-matched vessel" is a useful signal
                            //when reading the trace.
                            break;
                        case VesselLoadOutcome.Failed:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in CheckVesselsToLoad {e}");
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Sends a delayed vessel definition to the server.
        /// Call this method if you expect to do a lot of modifications to a vessel and you want to send it only once.
        /// <paramref name="reason"/> is forwarded to the server's craft create/remove audit log.
        /// </summary>
        public void DelayedSendVesselMessage(Guid vesselId, float delayInSec, bool forceReload = false, string reason = null)
        {
            if (QueuedVesselsToSend.Contains(vesselId)) return;

            QueuedVesselsToSend.Add(vesselId);
            CoroutineUtil.StartDelayedRoutine("QueueVesselMessageAsPartsChanged", () =>
            {
                QueuedVesselsToSend.Remove(vesselId);

                LunaLog.Log($"[LMP]: Sending delayed proto vessel {vesselId}");
                MessageSender.SendVesselMessage(FlightGlobals.FindVessel(vesselId), forceReload, reason);
            }, delayInSec);
        }

        /// <summary>
        /// Removes a vessel from the system
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            VesselProtos.TryRemove(vesselId, out _);
            //Drop the drift cache entry so a vessel re-created with the same id later
            //in the session (e.g. revert-to-launch from EDITOR, or a re-spawn after a
            //remote kill+resend) starts from a clean slate and the first legitimate
            //broadcast after the recreate isn't suppressed.
            LastBroadcastDriftPartCount.TryRemove(vesselId, out _);
            //A vessel id resurrected in the same session must not inherit a stale
            //"I just mutated locally" record from the previous incarnation, which
            //would suppress the first wire update on the new vessel.
            LocalTopologyTracker.ClearVessel(vesselId);

            //Best-effort name lookup so the trace stays human-readable; FindVessel
            //may have already returned null by the time we get here (e.g. when
            //RemoveVessel runs from the kill-vessel pipeline after the live
            //Vessel was destroyed).
            string vesselName = null;
            try { vesselName = FlightGlobals.FindVessel(vesselId)?.vesselName; } catch { /* swallow */ }
            VesselSyncDiagnostics.LogRemoved(vesselId, vesselName, reason: "VesselProtoSystem.RemoveVessel");
        }

        #endregion

        #region Diagnostic helpers

        /// <summary>
        /// Try/catch wrapper around <c>ProtoVessel.vesselName</c> so a half-loaded
        /// proto with a broken name field can't break the diagnostic write that
        /// is supposed to surface it. Returns null on failure; the writer
        /// substitutes its own placeholder.
        /// </summary>
        private static string SafeName(ProtoVessel proto)
        {
            try { return proto?.vesselName; }
            catch { return null; }
        }

        /// <summary>
        /// Try/catch wrapper around <c>ProtoVessel.protoPartSnapshots.Count</c>
        /// for the same reason as <see cref="SafeName"/>. Returns -1 on failure,
        /// which the writer renders as "?".
        /// </summary>
        private static int SafePartCount(ProtoVessel proto)
        {
            try { return proto?.protoPartSnapshots?.Count ?? -1; }
            catch { return -1; }
        }

        /// <summary>
        /// Try/catch wrapper around <c>ProtoVessel.situation</c>. Returns
        /// <see cref="Vessel.Situations.PRELAUNCH"/> as a benign fallback so
        /// the diagnostic write can never propagate a partial-proto exception.
        /// </summary>
        private static Vessel.Situations SafeSituation(ProtoVessel proto)
        {
            try { return proto?.situation ?? Vessel.Situations.PRELAUNCH; }
            catch { return Vessel.Situations.PRELAUNCH; }
        }

        #endregion
    }
}
