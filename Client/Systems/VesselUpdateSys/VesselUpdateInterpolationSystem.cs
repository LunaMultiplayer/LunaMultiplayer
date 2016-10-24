using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// Main interpolation system class for vessel updates
    /// </summary>
    public class VesselUpdateInterpolationSystem : SubSystem<VesselUpdateSystem>
    {
        #region Fields

        private const float FactorAdjustSecInterval = 0.5f;
        private const float FactorAdjustValue = 0.05f;
        private const float DefaultFactor = 1.7f;

        private const int MaxUpdatesInQueue = 4;
        private const int MinUpdatesInQueue = 2;

        private const float MaxSecWithuotUpdates = 10;
        private const float RemoveVesselsSecInterval = 5;

        /// <summary>
        /// The current vessel update that is being handled
        /// </summary>
        public Dictionary<Guid, VesselUpdate> CurrentVesselUpdate { get; } = new Dictionary<Guid, VesselUpdate>();

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
        /// Retrieves the interpolation factor for given vessel
        /// </summary>
        /// <param name="vesselId"></param>
        /// <returns></returns>
        public static float GetInterpolationFactor(Guid vesselId) => InterpolationLengthFactor.ContainsKey(vesselId)
            ? Time.fixedDeltaTime*InterpolationLengthFactor[vesselId]
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
        /// Main system that picks updates received and sets them for further processing. We call it in the 
        /// fixed update as in deals with phisics
        /// </summary>
        public void FixedUpdate()
        {
            foreach (
                var vesselUpdates in
                System.ReceivedUpdates.Where(v => InterpolationFinished(v.Key) && v.Value.Count > 0))
            {
                var success = !CurrentVesselUpdate.ContainsKey(vesselUpdates.Key)
                    ? SetFirstVesselUpdates(GetValidUpdate(vesselUpdates.Key, 0, vesselUpdates.Value))
                    : HandleVesselUpdate(vesselUpdates);

                if (success)
                    Client.Singleton.StartCoroutine(CurrentVesselUpdate[vesselUpdates.Key].ApplyVesselUpdate());
            }
        }

        /// <summary>
        /// This coroutine adjust the interpolation factor so the interpolation lengths are dynamically adjusted in order 
        /// to have maximum MaxUpdatesInQueue packets in the queue and MinUpdatesInQueue minimum
        /// </summary>
        /// <returns></returns>
        public IEnumerator AdjustInterpolationLengthFactor()
        {
            var seconds = new WaitForSeconds(FactorAdjustSecInterval);
            while (true)
            {
                try
                {
                    if (!System.Enabled)
                        break;

                    if (System.UpdateSystemReady)
                    {
                        foreach (var update in System.ReceivedUpdates)
                        {
                            if (System.GetNumberOfUpdatesInQueue(update.Key) > MaxUpdatesInQueue)
                                IncreaseInterpolationFactor(update.Key);
                            else if (System.GetNumberOfUpdatesInQueue(update.Key) < MinUpdatesInQueue)
                                DecreaseInterpolationFactor(update.Key);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in AdjustInterpolationLengthFactor {e}");
                }

                yield return seconds;
            }
        }

        /// <summary>
        /// Remove the vessels that didn't receive and update after the value specified in MsWithoutUpdatesToRemove every 5 seconds
        /// </summary>
        public IEnumerator RemoveVessels()
        {
            var seconds = new WaitForSeconds(RemoveVesselsSecInterval);
            while (true)
            {
                try
                {
                    if (!System.Enabled) break;

                    if (System.UpdateSystemReady)
                    {
                        var vesselsToRemove = CurrentVesselUpdate
                            .Where(u => u.Value.InterpolationFinished && Time.time - u.Value.FinishTime > MaxSecWithuotUpdates)
                            .Select(u => u.Key).ToArray();

                        foreach (var vesselId in vesselsToRemove)
                        {
                            InterpolationLengthFactor.Remove(vesselId);
                            CurrentVesselUpdate.Remove(vesselId);
                            System.ReceivedUpdates.Remove(vesselId);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in RemoveVessels {e}");
                }

                yield return seconds;
            }
        }

        #endregion

        #region Private

        /// <summary>
        /// Retrieves an update from the queue that was sent later than the one we have as target 
        /// and that was received close to "MSInthepast". Rmember to call it from fixed update only
        /// </summary>
        private static VesselUpdate GetValidUpdate(Guid vesselId, float targetSentTime, Queue<VesselUpdate> vesselUpdates)
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

        private bool HandleVesselUpdate(KeyValuePair<Guid, Queue<VesselUpdate>> vesselUpdates)
        {
            var update = GetValidUpdate(vesselUpdates.Key, CurrentVesselUpdate[vesselUpdates.Key].Target.SentTime, vesselUpdates.Value);
            if (update == null) return false;
            
            update.Vessel = CurrentVesselUpdate[vesselUpdates.Key].Vessel;
            if (CurrentVesselUpdate[vesselUpdates.Key].Target.BodyName == update.BodyName)
                update.Body = CurrentVesselUpdate[vesselUpdates.Key].Target.Body;

            CurrentVesselUpdate[vesselUpdates.Key] = CurrentVesselUpdate[vesselUpdates.Key].Target;
            CurrentVesselUpdate[vesselUpdates.Key].Target = update;

            return true;
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
        private bool SetFirstVesselUpdates(VesselUpdate update)
        {
            if (update == null) return false;

            var currentPosition = VesselUpdate.CreateFromVesselId(update.VesselId);
            if (currentPosition != null)
            {
                currentPosition.SentTime = update.SentTime - DefaultFactor;
                CurrentVesselUpdate.Add(update.VesselId, currentPosition);
                CurrentVesselUpdate[update.VesselId].Target = update;
                return true;
            }
            return false;
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

            InterpolationLengthFactor[vesselId] = Mathf.Clamp(InterpolationLengthFactor[vesselId], 1, 3);
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

            InterpolationLengthFactor[vesselId] = Mathf.Clamp(InterpolationLengthFactor[vesselId], 1, 3);
        }

        #endregion
    }
}
