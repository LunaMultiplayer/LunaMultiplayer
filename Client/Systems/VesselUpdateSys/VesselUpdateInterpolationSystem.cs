using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Network;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// Main interpolation system class for vessel updates
    /// </summary>
    public class VesselUpdateInterpolationSystem : SubSystem<VesselUpdateSystem>
    {
        #region Fields

        /// <summary>
        /// This variable specifies how many miliseconds in the past we work. It's based on the ping with a minimum of 500ms.
        /// For bad conections we will work several MS in the past as we need time to receive them.
        /// Good connections can have this value closer to 0 although it will never be 0.
        /// </summary>
        public static double MsInPast => NetworkStatistics.PingMs * 2 <= 500 ? 500 : NetworkStatistics.PingMs * 2;

        public static float SInPast => (float)TimeSpan.FromMilliseconds(MsInPast).TotalSeconds;

        /// <summary>
        /// After the value in ms specified here the vessel will be removed from the interpolation system
        /// </summary>
        private int MsWithoutUpdatesToRemove { get; } = 10000;

        /// <summary>
        /// The current vessel update that is being handled
        /// </summary>
        public Dictionary<Guid, VesselUpdate> CurrentVesselUpdate { get; } = new Dictionary<Guid, VesselUpdate>();

        #endregion

        /// <summary>
        /// Clear all the properties
        /// </summary>
        public void ResetSystem()
        {
            CurrentVesselUpdate.Clear();
        }

        /// <summary>
        /// Remove the vessels that didn't receive and update after the value specified in MsWithoutUpdatesToRemove
        /// </summary>
        public void RemoveVessels()
        {
            var vesselsToRemove = CurrentVesselUpdate
                .Where(u => u.Value.InterpolationFinished && TimeSpan.FromSeconds(Time.time - u.Value.FinishTime).TotalMilliseconds > MsWithoutUpdatesToRemove)
                .Select(u => u.Key).ToArray();

            foreach (var vesselId in vesselsToRemove)
            {
                CurrentVesselUpdate.Remove(vesselId);
                System.ReceivedUpdates.Remove(vesselId);
            }
        }

        /// <summary>
        /// Main system that picks updates received and sets them for further processing
        /// </summary>
        public void HandleVesselUpdates()
        {
            //Iterate over the updates that do not have interpolations going on
            foreach (var vesselUpdates in System.ReceivedUpdates.Where(v => InterpolationFinished(v.Key) && v.Value.Count > 0))
            {
                var success = !CurrentVesselUpdate.ContainsKey(vesselUpdates.Key) ?
                    SetFirstVesselUpdates(GetValidUpdate(0, vesselUpdates.Value)) :
                    HandleVesselUpdate(vesselUpdates);

                if (success)
                    Client.Singleton.StartCoroutine(CurrentVesselUpdate[vesselUpdates.Key].ApplyVesselUpdate());
            }
        }

        private VesselUpdate GetValidUpdate(long targetSentTime, Queue<VesselUpdate> vesselUpdates)
        {
            var update = vesselUpdates.ToList()
                .Where(u => u.SentTime > targetSentTime)
                .OrderBy(u => Math.Abs((Time.fixedTime - u.ReceiveTime) - SInPast))
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
            var update = GetValidUpdate(CurrentVesselUpdate[vesselUpdates.Key].Target.SentTime, vesselUpdates.Value);
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
                currentPosition.ReceiveTime = update.ReceiveTime - SInPast;
                CurrentVesselUpdate.Add(update.VesselId, currentPosition);
                CurrentVesselUpdate[update.VesselId].Target = update;
                return true;
            }
            return false;
        }
    }
}
