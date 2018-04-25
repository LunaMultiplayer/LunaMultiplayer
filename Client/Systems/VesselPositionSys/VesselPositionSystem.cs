using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselPositionSystem : MessageSystem<VesselPositionSystem, VesselPositionMessageSender, VesselPositionMessageHandler>
    {
        public const int MaxQueuedUpdates = 5;

        #region Fields & properties

        private static float LastVesselUpdatesSentTime { get; set; }
        private static int VesselUpdatesSendMsInterval => SettingsSystem.ServerSettings.VesselPositionUpdatesMsInterval;
        private static bool TimeToSendVesselUpdate => VesselCommon.PlayerVesselsNearby() ?
            TimeSpan.FromSeconds(Time.fixedTime - LastVesselUpdatesSentTime).TotalMilliseconds > VesselUpdatesSendMsInterval :
            TimeSpan.FromSeconds(Time.fixedTime - LastVesselUpdatesSentTime).TotalMilliseconds > 100;

        public bool PositionUpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public bool PositionUpdateSystemBasicReady => Enabled && PositionUpdateSystemReady || HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        public static ConcurrentDictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } =
            new ConcurrentDictionary<Guid, VesselPositionUpdate>();
        
        public static ConcurrentDictionary<Guid, FixedSizedConcurrentQueue<VesselPositionUpdate>> TargetVesselUpdateQueue { get; } =
            new ConcurrentDictionary<Guid, FixedSizedConcurrentQueue<VesselPositionUpdate>>();

        public static Queue<Guid> VesselsToRemove { get; } = new Queue<Guid>();

        private List<Vessel> SecondaryVesselsToUpdate { get; } = new List<Vessel>();
        private List<Vessel> AbandonedVesselsToUpdate { get; } = new List<Vessel>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselPositionSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.ObscenelyEarly, HandleVesselUpdates);
            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.BetterLateThanNever, SendVesselPositionUpdates);

            //It's important that SECONDARY vessels send their position in the UPDATE as their parameters will NOT be updated on the fixed update if the are packed.
            //https://forum.kerbalspaceprogram.com/index.php?/topic/173885-packed-vessels-position-isnt-reliable-from-fixedupdate/
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselPositionUpdatesMsInterval, RoutineExecution.Update, SendSecondaryVesselPositionUpdates));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselPositionUpdatesMsInterval, RoutineExecution.Update, SendUnloadedSecondaryVesselPositionUpdates));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CurrentVesselUpdate.Clear();
            TargetVesselUpdateQueue.Clear();

            TimingManager.UpdateRemove(TimingManager.TimingStage.ObscenelyEarly, HandleVesselUpdates);
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.ObscenelyEarly, SendVesselPositionUpdates);
        }

        private void HandleVesselUpdates()
        {
            if (!PositionUpdateSystemBasicReady) return;

            foreach (var keyVal in CurrentVesselUpdate)
            {
                if (!VesselCommon.DoVesselChecks(keyVal.Key))
                    RemoveVesselFromSystem(keyVal.Key);
                
                keyVal.Value.ApplyInterpolatedVesselUpdate();
            }

            while (VesselsToRemove.Count > 0)
            {
                var vesselToRemove = VesselsToRemove.Dequeue();
                CurrentVesselUpdate.TryRemove(vesselToRemove, out _);
                TargetVesselUpdateQueue.TryRemove(vesselToRemove, out _);
            }
        }

        #endregion

        #region FixedUpdate methods

        /// <summary>
        /// Send the updates of our own vessel. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselPositionUpdates()
        {
            if (PositionUpdateSystemReady && TimeToSendVesselUpdate && !VesselCommon.IsSpectating)
            {
                MessageSender.SendVesselPositionUpdate(FlightGlobals.ActiveVessel);
                LastVesselUpdatesSentTime = Time.fixedTime;
            }
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Send updates for vessels that we own the update lock. And also send it for the abandoned ones
        /// </summary>
        private void SendSecondaryVesselPositionUpdates()
        {
            if (PositionUpdateSystemReady && !VesselCommon.IsSpectating)
            {
                SecondaryVesselsToUpdate.Clear();
                SecondaryVesselsToUpdate.AddRange(VesselCommon.GetSecondaryVessels());

                for (var i = 0; i < SecondaryVesselsToUpdate.Count; i++)
                {
                    UpdateSecondaryVesselValues(SecondaryVesselsToUpdate[i]);
                    MessageSender.SendVesselPositionUpdate(SecondaryVesselsToUpdate[i]);
                }
            }
        }

        /// <summary>
        /// Send updates for vessels that we own the update lock. And also send it for the abandoned ones
        /// </summary>
        private void SendUnloadedSecondaryVesselPositionUpdates()
        {
            if (PositionUpdateSystemBasicReady && !VesselCommon.IsSpectating)
            {
                AbandonedVesselsToUpdate.Clear();
                AbandonedVesselsToUpdate.AddRange(VesselCommon.GetUnloadedSecondaryVessels());

                for (var i = 0; i < AbandonedVesselsToUpdate.Count; i++)
                {
                    UpdateUnloadedVesselValues(AbandonedVesselsToUpdate[i]);
                    MessageSender.SendVesselPositionUpdate(AbandonedVesselsToUpdate[i]);
                }
            }
        }

        #endregion

        #region Public methods
        
        /// <summary>
        /// Gets the latest received position of a vessel
        /// </summary>
        public double[] GetLatestVesselPosition(Guid vesselId)
        {
            return TargetVesselUpdateQueue.TryGetValue(vesselId, out var vesselPositionQueue) ? 
                vesselPositionQueue.TryPeek(out var vesselPos) ? vesselPos.LatLonAlt :
                CurrentVesselUpdate.TryGetValue(vesselId, out vesselPos) ?
                    vesselPos.LatLonAlt :
                    null : null;
        }

        /// <summary>
        /// Removes a vessel from the system
        /// </summary>
        public void RemoveVesselFromSystem(Guid vesselId)
        {
            VesselsToRemove.Enqueue(vesselId);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Unloaded vessels don't update their lat/lon/alt and it's orbit params.
        /// As we have the unloadedupdate lock of that vessel we need to refresh those values manually
        /// </summary>
        public static void UpdateUnloadedVesselValues(Vessel vessel)
        {
            if (vessel.orbit != null)
            {
                vessel.orbit?.UpdateFromStateVectors(vessel.orbit.pos, vessel.orbit.vel, vessel.orbit.referenceBody, Planetarium.GetUniversalTime());
                if (!vessel.LandedOrSplashed)
                {
                    vessel.mainBody.GetLatLonAltOrbital(vessel.orbit.pos, out vessel.latitude, out vessel.longitude, out vessel.altitude);
                    vessel.orbitDriver?.updateFromParameters();
                }
            }
        }

        /// <summary>
        /// Secondary vessels (vessels that are loaded and we have the update lock) don't get their lat/lon/alt values updated by the game engine
        /// Here we manually update them so we send updateded lat/lon/alt values
        /// </summary>
        public static void UpdateSecondaryVesselValues(Vessel vessel)
        {
            vessel.srfRelRotation = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.vesselTransform.rotation;
            if (vessel.LandedOrSplashed)
            {
                if (!vessel.packed) //If you do it and the vessel is packed the vessel will start flying to the sky
                    vessel.mainBody.GetLatLonAlt(vessel.GetWorldPos3D(), out vessel.latitude, out vessel.longitude, out vessel.altitude);
            }
            else
            {
                if (vessel.orbit != null)
                    vessel.mainBody.GetLatLonAltOrbital(vessel.orbit.pos, out vessel.latitude, out vessel.longitude, out vessel.altitude);
            }

            vessel.UpdatePosVel();
            vessel.precalc.CalculatePhysicsStats();
        }

        #endregion
    }
}
