using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Network;

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
        public double MsInPast { get; set; } = NetworkStatistics.PingMs * 2 <= 500 ? 500: NetworkStatistics.PingMs * 2;
        
        /// <summary>
        /// After the value in ms specified here the vessel will be removed from the interpolation system
        /// </summary>
        private int MsWithoutUpdatesToRemove { get; } = 10000;

        /// <summary>
        /// Interpolattion must be hadnled in the same tick number so here we specify it
        /// </summary>
        private long InterpolationTick { get; set; }

        /// <summary>
        /// The current vessel update that is being handled
        /// </summary>
        public Dictionary<Guid, VesselUpdate> CurrentVesselUpdate { get; } = new Dictionary<Guid, VesselUpdate>();

        /// <summary>
        /// The update that the interpolation is going to. After the interpolation is finished, we will 
        /// set here a new value and move the old one to CurrentVesselUpdate
        /// </summary>
        public Dictionary<Guid, VesselUpdate> NextVesselUpdate { get; } = new Dictionary<Guid, VesselUpdate>();

        #endregion

        /// <summary>
        /// Clear all the properties
        /// </summary>
        public void ResetSystem()
        {
            CurrentVesselUpdate.Clear();
            NextVesselUpdate.Clear();
        }

        /// <summary>
        /// Remove the vessels that didn't receive and update after the value specified in MsWithoutUpdatesToRemove
        /// </summary>
        public void RemoveVessels()
        {
            var vesselsToRemove = CurrentVesselUpdate
                .Where(u => u.Value.InterpolationFinishTime > 0 && TimeSpan.FromTicks(DateTime.UtcNow.Ticks - u.Value.InterpolationFinishTime).TotalMilliseconds > MsWithoutUpdatesToRemove)
                .Select(u => u.Key).ToArray();

            foreach (var vesselId in vesselsToRemove)
            {
                CurrentVesselUpdate.Remove(vesselId);
                NextVesselUpdate.Remove(vesselId);
                System.ReceivedUpdates.Remove(vesselId);
            }
        }
        
        /// <summary>
        /// Main system that picks updates received and sets them for further processing
        /// </summary>
        public void HandleVesselUpdates()
        {
            //Iterate over the updates that do not have interpolations going on
            foreach (var vesselUpdates in System.ReceivedUpdates.Where(v => InterpolationFinished(v.Key) && v.Value.Any()).Select(u => u.Value))
            {
                InterpolationTick = DateTime.UtcNow.Ticks;
                RemoveOldUpdates(vesselUpdates);

                //Here we get the oldest update from the list.
                //Usually it will be a packet with a sent time close to the value of MilisecondsInPast
                var update = vesselUpdates.OrderBy(u => u.ReceiveTime).FirstOrDefault();

                if (update != null)
                {
                    HandleVesselUpdate(update);
                    if (CurrentVesselUpdate.ContainsKey(update.VesselId) && NextVesselUpdate.ContainsKey(update.VesselId))
                        Client.Singleton.StartCoroutine(CurrentVesselUpdate[update.VesselId].ApplyVesselUpdate(NextVesselUpdate[update.VesselId]));
                }
            }
        }
        
        private void HandleVesselUpdate(VesselUpdate update)
        {
            var vesselId = update.VesselId;
            if (!NextVesselUpdate.ContainsKey(vesselId))
            {
                SetFirstVesselUpdates(update);
                return;
            }

            var tdifference = update.SentTime - NextVesselUpdate[vesselId].SentTime;
            if (tdifference > 0)
            {
                CurrentVesselUpdate[vesselId].InterpolationFinishTime = InterpolationTick;

                //This variable shows the REAL time we took to interpolate minus the supposed interpolation duration. 
                //It's normal that it takes more time as we have to take the CPU time into account
                var extraTime = CurrentVesselUpdate[vesselId].InterpolationFinishTime -
                                CurrentVesselUpdate[vesselId].InterpolationStartTime -
                                CurrentVesselUpdate[vesselId].InterpolationDuration;

                var interpolationDurationWithExtraTime = update.ReceiveTime - NextVesselUpdate[vesselId].ReceiveTime - extraTime;
                if (interpolationDurationWithExtraTime <= 0)
                {
                    //This means that the interpolation we are trying to do has a smaller duration timespan than the extra time we took before.
                    //Therefore, we must drop this update and try with some future update that was received more recently and increase that durationtimespan.
                    //When you do this, you must compensate and increase the duration of this interpolation 
                    //as you're using a packet that you're not supposed to use as its send time is less than MsInPast.
                    var validUpdate = System.ReceivedUpdates[vesselId]
                        .FirstOrDefault(u => u.ReceiveTime - NextVesselUpdate[vesselId].ReceiveTime - extraTime > 0);

                    if (validUpdate != null)
                    {
                        var timeDifference = validUpdate.ReceiveTime - update.ReceiveTime;
                        HandleVesselUpdate(validUpdate);
                        CurrentVesselUpdate[vesselId].InterpolationDuration += timeDifference;
                    }
                    return;
                }
                else
                {
                    CurrentVesselUpdate[vesselId] = NextVesselUpdate[vesselId];
                    NextVesselUpdate[vesselId] = update;

                    //Remove the extra time from the last interpolation from the future interpolation duration
                    CurrentVesselUpdate[vesselId].InterpolationDuration -= extraTime;
                    CurrentVesselUpdate[vesselId].InterpolationStartTime = InterpolationTick;
                }
            }
            else
            {
                var validUpdate = System.ReceivedUpdates[vesselId].FirstOrDefault(u => update.SentTime - NextVesselUpdate[vesselId].SentTime > 0);

                if (validUpdate != null)
                {
                    HandleVesselUpdate(validUpdate);
                    return;
                }
            }
        }

        /// <summary>
        /// This method removes the updates that are older than the value we specified in MilisecondsInPast + 10%.
        /// </summary>
        /// <param name="vesselUpdates"></param>
        private void RemoveOldUpdates(ICollection<VesselUpdate> vesselUpdates)
        {
            var ticksInPastPlus10Percent = TimeSpan.FromMilliseconds(MsInPast + (MsInPast * 10 / 100)).Ticks;
            var updatesToRemove = vesselUpdates.Where(u => u.ReceiveTime < InterpolationTick - ticksInPastPlus10Percent).ToList();
            foreach (var updatetoRemove in updatesToRemove)
                vesselUpdates.Remove(updatetoRemove);
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
        private void SetFirstVesselUpdates(VesselUpdate update)
        {
            var currentPosition = VesselUpdate.CreateFromVesselId(update.VesselId);
            if (currentPosition != null)
            {
                currentPosition.ReceiveTime = update.ReceiveTime - TimeSpan.FromMilliseconds(MsInPast).Ticks;
                CurrentVesselUpdate.Add(update.VesselId, currentPosition);

                NextVesselUpdate.Add(update.VesselId, update);
                CurrentVesselUpdate[update.VesselId].InterpolationStartTime = DateTime.UtcNow.Ticks;
            }
        }
    }
}
