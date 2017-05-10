using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionSys
{
    /// <summary>
    /// Main interpolation system class for vessel updates
    /// </summary>
    public class VesselPositionInterpolationSystem : SubSystem<VesselPositionSystem>
    {
        #region Fields

        private const float FactorAdjustSecInterval = 0.5f;
        private const float FactorAdjustValue = 0.05f;
        private const float DefaultFactor = 1.7f;

        public const int MaxTotalUpdatesInQueue = 5;
        public const float MaxSInterpolationTime = 0.5f;
        private const int MaxUpdatesInQueue = 4;
        private const int MinUpdatesInQueue = 2;

        /// <summary>
        /// The vessel update that are being handled for each vessel
        /// </summary>
        public Dictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } = new Dictionary<Guid, VesselPositionUpdate>();



        /// <summary>
        /// This dictioanry control the length of the interpolations for each vessel.
        /// If the value is big, then the interpolation will last less so we will consume faster the updates in the queue.
        /// If the value is small then the interpolation will last longer and we will consume packets more slowly.
        /// The idea is to have a buffer with some packets but not too many. (Between MinUpdatesInQueue and MaxUpdatesInQueue)
        /// </summary>
        private static readonly Dictionary<Guid, float> InterpolationLengthFactor = new Dictionary<Guid, float>();

        #endregion

        #region Public

        /// <summary>
        /// Remove a vessel from the system
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            InterpolationLengthFactor.Remove(vesselId);
            CurrentVesselUpdate.Remove(vesselId);
        }

        /// <summary>
        /// Retrieves the interpolation factor for given vessel
        /// </summary>
        /// <param name="vesselId"></param>
        /// <returns></returns>
        public static float GetInterpolationFactor(Guid vesselId) => InterpolationLengthFactor.ContainsKey(vesselId)
            ? Time.fixedDeltaTime * InterpolationLengthFactor[vesselId]
            : 0;

        /// <summary>
        /// Clear all the properties
        /// </summary>
        public void ResetSystem()
        {
            CurrentVesselUpdate.Clear();
            InterpolationLengthFactor.Clear();
        }

        /// <summary>
        /// Main system that picks updates received and sets them for further processing. 
        /// </summary>
        public void Update()
        {
            foreach (var vesselUpdates in System.ReceivedUpdates.Where(v => InterpolationFinished(v.Key)))
            {
                if (!CurrentVesselUpdate.ContainsKey(vesselUpdates.Key))
                {
                    SetFirstVesselUpdates(vesselUpdates.Value);
                }
                else
                {
                    HandleVesselUpdate(vesselUpdates.Value);
                }
            }

            //Run through all the updates that are not finished and apply them
            foreach (var vesselUpdates in CurrentVesselUpdate.Where(u => !u.Value.InterpolationFinished))
            {
                vesselUpdates.Value.ApplyVesselUpdate();
            }
        }
        
        #endregion

        #region Private

        /// <summary>
        /// Retrieves an update from the queue that was sent later than the one we have as target 
        /// and that was received close to "MSInthepast". Rmember to call it from fixed update only
        /// </summary>
        private static VesselPositionUpdate GetValidUpdate(Guid vesselId, float targetSentTime, Queue<VesselPositionUpdate> vesselUpdates)
        {
            var update = vesselUpdates.ToList()
                .Where(u => u.SentTime > targetSentTime && (u.SentTime - targetSentTime - GetInterpolationFactor(vesselId)) > 0)
                .OrderBy(u => u.SentTime)
                .FirstOrDefault();

            if (update != null)
            {
                var dequeued = vesselUpdates.Dequeue();
                while (dequeued.Id != update.Id)
                    dequeued = vesselUpdates.Dequeue();
            }

            return update;
        }

        /// <summary>
        /// Sets the old target as the update and the dequeued new update as it's target
        /// </summary>
        /// TODO: Remove
        private void HandleVesselUpdate(KeyValuePair<Guid, Queue<VesselPositionUpdate>> vesselUpdates)
        {
            var update = GetValidUpdate(vesselUpdates.Key, CurrentVesselUpdate[vesselUpdates.Key].Target.SentTime, vesselUpdates.Value);
            if (update == null)
                return;

            update.Vessel = CurrentVesselUpdate[vesselUpdates.Key].Vessel;
            if (CurrentVesselUpdate[vesselUpdates.Key].Target.BodyName == update.BodyName)
                update.Body = CurrentVesselUpdate[vesselUpdates.Key].Target.Body;

            CurrentVesselUpdate[vesselUpdates.Key] = CurrentVesselUpdate[vesselUpdates.Key].Target;
            CurrentVesselUpdate[vesselUpdates.Key].Target = update;
        }

        private void HandleVesselUpdate(VesselPositionUpdate vesselUpdate)
        {
            var key = vesselUpdate.VesselId;
            if (CurrentVesselUpdate[key].SentTime < vesselUpdate.SentTime)
            {
                vesselUpdate.Vessel = CurrentVesselUpdate[key].Vessel;
                if (CurrentVesselUpdate[key].Target.BodyName == vesselUpdate.BodyName)
                    vesselUpdate.Body = CurrentVesselUpdate[key].Target.Body;

                CurrentVesselUpdate[key] = CurrentVesselUpdate[key].Target;
                CurrentVesselUpdate[key].Target = vesselUpdate;
            }
        }

        /// <summary>
        /// Check if the given vesselId has finished it's interpolation
        /// </summary>
        /// <param name="vesselId"></param>
        /// <returns></returns>
        private bool InterpolationFinished(Guid vesselId)
        {
            if (CurrentVesselUpdate.ContainsKey(vesselId) && CurrentVesselUpdate[vesselId] != null)
            {
                return CurrentVesselUpdate[vesselId].InterpolationStarted && CurrentVesselUpdate[vesselId].InterpolationFinished;
            }
            return true;
        }

        /// <summary>
        /// Here we set the first vessel updates. We use the current vessel state as the starting point.
        /// </summary>
        /// <param name="update"></param>
        private void SetFirstVesselUpdates(VesselPositionUpdate update)
        {
            var first = update?.Clone();
            if (first != null)
            {
                first.SentTime = update.SentTime - DefaultFactor;
                CurrentVesselUpdate.Add(update.VesselId, first);
                CurrentVesselUpdate[update.VesselId].Target = update;
            }
        }

        /// <summary>
        /// Increases the interpolation factor for the given vessel so we consume faster the updates
        /// </summary>
        private static void IncreaseInterpolationFactor(Guid vesselId)
        {
            if (!InterpolationLengthFactor.ContainsKey(vesselId))
                InterpolationLengthFactor.Add(vesselId, DefaultFactor);
            else
                InterpolationLengthFactor[vesselId] += FactorAdjustValue;

            InterpolationLengthFactor[vesselId] = Mathf.Clamp(InterpolationLengthFactor[vesselId], 1, 100);
        }

        /// <summary>
        /// Decreases the interpolation factor for the given vessel so we consume slower the updates
        /// </summary>
        private static void DecreaseInterpolationFactor(Guid vesselId)
        {
            if (!InterpolationLengthFactor.ContainsKey(vesselId))
                InterpolationLengthFactor.Add(vesselId, DefaultFactor);
            else
                InterpolationLengthFactor[vesselId] -= FactorAdjustValue;

            InterpolationLengthFactor[vesselId] = Mathf.Clamp(InterpolationLengthFactor[vesselId], 1, 100);
        }

        #endregion
    }
}