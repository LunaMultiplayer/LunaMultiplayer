using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.TimeSync;
using LmpClient.VesselUtilities;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace LmpClient.Systems.VesselPositionSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselPositionSystem : MessageSystem<VesselPositionSystem, VesselPositionMessageSender, VesselPositionMessageHandler>
    {
        #region Fields & properties
        
        private static DateTime LastVesselUpdatesSentTime { get; set; } = LunaComputerTime.UtcNow;

        private static int UpdateIntervalLockedToUnity => (int)(Math.Floor(SettingsSystem.ServerSettings.VesselUpdatesMsInterval 
            / TimeSpan.FromSeconds(Time.fixedDeltaTime).TotalMilliseconds) * TimeSpan.FromSeconds(Time.fixedDeltaTime).TotalMilliseconds);

        private static int SecondaryVesselUpdatesUpdateIntervalLockedToUnity => 2 * (int)(Math.Floor(SettingsSystem.ServerSettings.VesselUpdatesMsInterval
            / TimeSpan.FromSeconds(Time.fixedDeltaTime).TotalMilliseconds) * TimeSpan.FromSeconds(Time.fixedDeltaTime).TotalMilliseconds);

        private static bool TimeToSendVesselUpdate => VesselCommon.PlayerVesselsNearby() ?
            (LunaComputerTime.UtcNow - LastVesselUpdatesSentTime).TotalMilliseconds > UpdateIntervalLockedToUnity :
            (LunaComputerTime.UtcNow - LastVesselUpdatesSentTime).TotalMilliseconds > SecondaryVesselUpdatesUpdateIntervalLockedToUnity;

        public bool PositionUpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public PositionEvents PositionEvents { get; } = new PositionEvents();

        public bool PositionUpdateSystemBasicReady => Enabled && PositionUpdateSystemReady || HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        public static ConcurrentDictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } =
            new ConcurrentDictionary<Guid, VesselPositionUpdate>();

        public static ConcurrentDictionary<Guid, PositionUpdateQueue> TargetVesselUpdateQueue { get; } =
            new ConcurrentDictionary<Guid, PositionUpdateQueue>();

        private List<Vessel> SecondaryVesselsToUpdate { get; } = new List<Vessel>();
        private List<Vessel> AbandonedVesselsToUpdate { get; } = new List<Vessel>();

        private const TimingManager.TimingStage HandlePositionsStage = TimingManager.TimingStage.BetterLateThanNever;
        private const TimingManager.TimingStage SendPositionsStage = TimingManager.TimingStage.BetterLateThanNever;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselPositionSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            TimingManager.FixedUpdateAdd(HandlePositionsStage, HandleVesselUpdates);

            //Send the position updates after all the calculations are done. If you send it in the fixed update sometimes weird rubber banding appear (specially in space)
            TimingManager.LateUpdateAdd(SendPositionsStage, SendVesselPositionUpdates);
            
            //It's important that SECONDARY vessels send their position in the UPDATE as their parameters will NOT be updated on the fixed update if the are packed.
            //https://forum.kerbalspaceprogram.com/index.php?/topic/173885-packed-vessels-position-isnt-reliable-from-fixedupdate/
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval, RoutineExecution.Update, SendSecondaryVesselPositionUpdates));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval, RoutineExecution.Update, SendUnloadedSecondaryVesselPositionUpdates));

            WarpEvent.onTimeWarpStopped.Add(PositionEvents.WarpStopped);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            
            TimingManager.FixedUpdateRemove(HandlePositionsStage, HandleVesselUpdates);
            TimingManager.LateUpdateRemove(SendPositionsStage, SendVesselPositionUpdates);

            CurrentVesselUpdate.Clear();
            TargetVesselUpdateQueue.Clear();
        }

        private void HandleVesselUpdates()
        {
            Profiler.BeginSample(nameof(HandleVesselUpdates));
            
            foreach (var keyVal in CurrentVesselUpdate)
            {
                keyVal.Value.ApplyInterpolatedVesselUpdate();
            }

            Profiler.EndSample();
        }

        #endregion

        #region FixedUpdate methods

        /// <summary>
        /// Send the updates of our own vessel. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselPositionUpdates()
        {
            Profiler.BeginSample(nameof(SendVesselPositionUpdates));

            if (PositionUpdateSystemReady && TimeToSendVesselUpdate && !VesselCommon.IsSpectating)
            {
                MessageSender.SendVesselPositionUpdate(FlightGlobals.ActiveVessel);
                LastVesselUpdatesSentTime = LunaComputerTime.UtcNow;
            }

            Profiler.EndSample();
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
            if (CurrentVesselUpdate.TryGetValue(vesselId, out var vesselPos))
            {
                var fullData = new double[vesselPos.LatLonAlt.Length + vesselPos.Orbit.Length];
                vesselPos.LatLonAlt.CopyTo(fullData, 0);
                vesselPos.Orbit.CopyTo(fullData, vesselPos.LatLonAlt.Length);
                return fullData;
            }
            return null;
        }

        /// <summary>
        /// Removes a vessel from the system
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            CurrentVesselUpdate.TryRemove(vesselId, out _);
            TargetVesselUpdateQueue.TryRemove(vesselId, out _);
        }

        /// <summary>
        /// Force adjustment of interpolation times
        /// </summary>
        public void AdjustExtraInterpolationTimes()
        {
            //Remove the target of the current update so basically the interpolation is stopped and on the next frame we pick a new one
            foreach (var keyVal in CurrentVesselUpdate)
            {
                keyVal.Value.Target = null;
            }

            //Now cleanup the target dictionary of old positions
            foreach (var keyVal in TargetVesselUpdateQueue)
            {
                while (keyVal.Value.TryPeek(out var targetUpd) && PositionUpdateIsTooOld(targetUpd))
                    keyVal.Value.TryDequeue(out _);
            }
        }

        #endregion

        #region Private methods

        private static bool PositionUpdateIsTooOld(VesselPositionUpdate update)
        {
            return update.GameTimeStamp < TimeSyncSystem.UniversalTime - VesselCommon.PositionAndFlightStateMessageOffsetSec(update.PingMs);
        }

        /// <summary>
        /// Unloaded vessels don't update their lat/lon/alt and it's orbit params.
        /// As we have the unloadedupdate lock of that vessel we need to refresh those values manually
        /// </summary>
        public static void UpdateUnloadedVesselValues(Vessel vessel)
        {
            if (vessel.orbit != null)
            {
                vessel.UpdatePosVel();
                if (!vessel.LandedOrSplashed)
                {
                    if (vessel.orbitDriver) vessel.orbitDriver.UpdateOrbit();
                    vessel.mainBody.GetLatLonAltOrbital(vessel.orbit.pos, out vessel.latitude, out vessel.longitude, out vessel.altitude);
                }
            }
        }

        /// <summary>
        /// Secondary vessels (vessels that are loaded and we have the update lock) don't get their lat/lon/alt values updated by the game engine
        /// Here we manually update them so we send updateded lat/lon/alt values
        /// </summary>
        public static void UpdateSecondaryVesselValues(Vessel vessel)
        {
            vessel.UpdateLandedSplashed();
            vessel.UpdatePosVel();

            if (vessel.orbitDriver) vessel.orbitDriver.UpdateOrbit();
            vessel.srfRelRotation = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.vesselTransform.rotation;

            vessel.mainBody.GetLatLonAlt(vessel.vesselTransform.position, out vessel.latitude, out vessel.longitude, out vessel.altitude);
        }

        #endregion
    }
}
