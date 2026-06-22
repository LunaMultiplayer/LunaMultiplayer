using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
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

        /// <summary>
        /// Tracks last-sent maneuver node signatures per vessel to detect dV changes between periodic ticks.
        /// Key: vessel ID. Value: concatenated UT|dV string for all nodes, or empty if no nodes.
        /// </summary>
        private static readonly Dictionary<Guid, string> ManeuverSignatures = new Dictionary<Guid, string>();

        public readonly HashSet<Guid> VesselsUnableToLoad = new HashSet<Guid>();

        public ConcurrentDictionary<Guid, VesselProtoQueue> VesselProtos { get; } = new ConcurrentDictionary<Guid, VesselProtoQueue>();

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

            GameEvents.onManeuverAdded.Add(VesselProtoEvents.ManeuverNodeAdded);
            GameEvents.onManeuverRemoved.Add(VesselProtoEvents.ManeuverNodeRemoved);
            
            // Attempt to load all the from-future messages we've been storing
            TimingManager.UpdateAdd(TimingManager.TimingStage.BetterLateThanNever, MessageHandler.OnUpdate);

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.Update, CheckVesselsToLoad));
            SetupRoutine(new RoutineDefinition(2500, RoutineExecution.Update, SendVesselDefinition));
            
#if DEBUG
            // Debug logging (every 2 minutes)
            SetupRoutine(new RoutineDefinition(120000, RoutineExecution.Update, MessageHandler.LogQueuedMessagesSize));
#endif
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

            GameEvents.onManeuverAdded.Remove(VesselProtoEvents.ManeuverNodeAdded);
            GameEvents.onManeuverRemoved.Remove(VesselProtoEvents.ManeuverNodeRemoved);

            TimingManager.UpdateRemove(TimingManager.TimingStage.BetterLateThanNever, MessageHandler.OnUpdate);

            //This is the main system that handles the vesselstore so if it's disabled clear the store too
            VesselProtos.Clear();
            VesselsUnableToLoad.Clear();
            QueuedVesselsToSend.Clear();
            ManeuverSignatures.Clear();
        }

        #endregion

        #region Update routines

        /// <summary>
        /// Send the definition of our own vessel and the secondary vessels.
        /// Also detects maneuver node dV changes (which fire no KSP event) and re-sends when they differ.
        /// </summary>
        private void SendVesselDefinition()
        {
            try
            {
                if (ProtoSystemReady)
                {
                    var activeVessel = FlightGlobals.ActiveVessel;

                    if (activeVessel.parts.Count != activeVessel.protoVessel.protoPartSnapshots.Count)
                        MessageSender.SendVesselMessage(activeVessel);

                    CheckAndSendManeuverChanges(activeVessel);

                    foreach (var vessel in VesselCommon.GetSecondaryVessels())
                    {
                        if (vessel.parts.Count != vessel.protoVessel.protoPartSnapshots.Count)
                            MessageSender.SendVesselMessage(vessel);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error in SendVesselDefinition {e}");
            }
        }

        /// <summary>
        /// Compares the current maneuver node state of a vessel against the last-sent snapshot.
        /// Sends the vessel proto if anything has changed (node added, removed, or dV edited).
        /// Only acts when we hold the update lock for this vessel.
        /// </summary>
        private void CheckAndSendManeuverChanges(Vessel vessel)
        {
            if (vessel == null) return;
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            var sig = GetManeuverSignature(vessel);
            if (ManeuverSignatures.TryGetValue(vessel.id, out var lastSig))
            {
                if (lastSig != sig)
                {
                    ManeuverSignatures[vessel.id] = sig;
                    LunaLog.Log($"[LMP]: Maneuver nodes changed on {vessel.vesselName}, sending updated proto");
                    MessageSender.SendVesselMessage(vessel);
                }
            }
            else
            {
                // First poll for this vessel — record baseline without sending
                ManeuverSignatures[vessel.id] = sig;
            }
        }

        /// <summary>
        /// Produces a compact string signature of all maneuver nodes on a vessel.
        /// Format: "UT|dVx,dVy,dVz;UT|dVx,dVy,dVz;...". Empty string if no nodes.
        /// </summary>
        private static string GetManeuverSignature(Vessel vessel)
        {
            var nodes = vessel?.patchedConicSolver?.maneuverNodes;
            if (nodes == null || nodes.Count == 0) return string.Empty;

            var parts = new string[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                var dv = nodes[i].DeltaV;
                parts[i] = $"{nodes[i].UT:F1}|{dv.x:F4},{dv.y:F4},{dv.z:F4}";
            }
            return string.Join(";", parts);
        }

        /// <summary>
        /// Check vessels that must be loaded
        /// </summary>
        public void CheckVesselsToLoad()
        {
            if (HighLogic.LoadedScene < GameScenes.SPACECENTER) return;

            try
            {
                foreach (var keyVal in VesselProtos)
                {
                    if (keyVal.Value.TryPeek(out var vesselProto) && vesselProto.GameTime <= TimeSyncSystem.UniversalTime)
                    {
                        keyVal.Value.TryDequeue(out _);

                        if (VesselRemoveSystem.VesselWillBeKilled(vesselProto.VesselId))
                            continue;

                        var forceReload = vesselProto.ForceReload;
                        var protoVessel = vesselProto.CreateProtoVessel();
                        keyVal.Value.Recycle(vesselProto);

                        var verboseErrors = !VesselsUnableToLoad.Contains(vesselProto.VesselId);
                        if (protoVessel == null || !protoVessel.Validate(verboseErrors) || protoVessel.HasInvalidParts(verboseErrors))
                        {
                            VesselsUnableToLoad.Add(vesselProto.VesselId);
                            continue;
                        }

                        VesselsUnableToLoad.Remove(vesselProto.VesselId);

                        var existingVessel = FlightGlobals.FindVessel(vesselProto.VesselId);
                        if (existingVessel == null)
                        {
                            if (VesselLoader.LoadVessel(protoVessel, forceReload))
                            {
                                LunaLog.Log($"[LMP]: Vessel {protoVessel.vesselID} loaded");
                                VesselLoadEvent.onLmpVesselLoaded.Fire(protoVessel.vesselRef);
                            }
                        }
                        else
                        {
                            if (VesselLoader.LoadVessel(protoVessel, forceReload))
                            {
                                LunaLog.Log($"[LMP]: Vessel {protoVessel.vesselID} reloaded");
                                VesselReloadEvent.onLmpVesselReloaded.Fire(protoVessel.vesselRef);
                            }
                        }
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
        /// Call this method if you expect to do a lot of modifications to a vessel and you want to send it only once
        /// </summary>
        public void DelayedSendVesselMessage(Guid vesselId, float delayInSec, bool forceReload = false)
        {
            if (QueuedVesselsToSend.Contains(vesselId)) return;

            QueuedVesselsToSend.Add(vesselId);
            CoroutineUtil.StartDelayedRoutine("QueueVesselMessageAsPartsChanged", () =>
            {
                QueuedVesselsToSend.Remove(vesselId);

                LunaLog.Log($"[LMP]: Sending delayed proto vessel {vesselId}");
                MessageSender.SendVesselMessage(FlightGlobals.FindVessel(vesselId));
            }, delayInSec);
        }

        /// <summary>
        /// Removes a vessel from the system
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            VesselProtos.TryRemove(vesselId, out _);
        }

        #endregion
    }
}
