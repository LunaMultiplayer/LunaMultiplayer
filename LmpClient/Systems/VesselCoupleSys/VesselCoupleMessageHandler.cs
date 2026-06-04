using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LmpClient.Systems.VesselCoupleSys
{
    public class VesselCoupleMessageHandler : SubSystem<VesselCoupleSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public SortedList<double,VesselCoupleMsgData> StoredMessagesData { get; set; } = new SortedList<double,VesselCoupleMsgData>();

        private readonly object StoredMessagesLock = new object();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselCoupleMsgData msgData)) return;

            // We don't call VesselCommon.DoVesselChecks(msgData.VesselId) because we may receive a 
            // proto update on our own vessel (when someone docks against us and we don't detect it for example
            // Therefore, we must manually call VesselWillBeKilled and implement only 1 of the checks
            if (VesselRemoveSystem.Singleton.VesselWillBeKilled(msgData.VesselId))
                return;
            

            var affectsActiveVessel = FlightGlobals.ActiveVessel && (FlightGlobals.ActiveVessel.id == msgData.VesselId || FlightGlobals.ActiveVessel.id == msgData.CoupledVesselId);

            // If the coupling packet affects our active vessel (even if we are spectating) jump to the future subspace
            if (affectsActiveVessel)
            {
                LunaLog.Log($"Received a coupling against our own vessel! We own the {(FlightGlobals.ActiveVessel.id == msgData.VesselId ? "Dominant" : "Weak")} vessel");
                WarpSystem.Singleton.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);
            }

            // This code helps prevent time paradoxes.
            // Why? Because this code makes sure that we only apply updates that aren't from the future.
            // Additionally, to prevent us from getting desynchronised, we store updates from the future, so that we can apply them later.
            var IsFromFuture = VesselCommon.UpdateIsFromFuture(msgData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime);

            if (IsFromFuture && !affectsActiveVessel)
            {
                double delta = 0;
                lock (StoredMessagesLock)
                {
                    while(true)
                    {
                        try
                        {
                            StoredMessagesData.Add(msgData.GameTime - delta, msgData);
                            break;
                        }
                        catch (ArgumentException)
                        {
                            // If we recieve another message of the same type, with the same game time, apply a small delta so that it's not exactly the same
                            // These floating point precision issues happen anyways so it shouldn't be too big of an issue
                            delta += 0.001;
                        }
                    }
                }
                return;
            }
            
            TryQueueUpdate(msgData);
        }
        
        public void TryQueueUpdate(VesselCoupleMsgData msgData)
        {
            if (!System.VesselCouples.ContainsKey(msgData.VesselId))
            {
                System.VesselCouples.TryAdd(msgData.VesselId, new VesselCoupleQueue());
            }

            if (System.VesselCouples.TryGetValue(msgData.VesselId, out var queue))
            {
                if (queue.TryPeek(out var value) && value.GameTime > msgData.GameTime)
                {
                    //A user reverted, so clear their message queue and start from scratch
                    queue.Clear();
                }

                if (msgData.GameTime <= TimeSyncSystem.UniversalTime)
                {
                    VesselCouple.ProcessCouple(msgData);
                }
                else
                {
                    queue.Enqueue(msgData);
                }
            }
        }

        // Search through all stored updates and apply any that are no longer from the future
        public void OnUpdate()
        {
            var softDeleted = new List<double>();

            // Lock the SortedList of stored messages
            lock (StoredMessagesLock)
            {
                foreach (var kvp in StoredMessagesData)
                {
                    // Ensure that the update is no longer considered to be from the future
                    if (VesselCommon.UpdateIsFromFuture(kvp.Key, WarpSystem.Singleton.CurrentSubspaceTime))
                    {
                        break;
                    }

                    // Soft delete the key-value pair
                    softDeleted.Add(kvp.Key);

                    // Apply the update
                    TryQueueUpdate(kvp.Value);
                }

                if (softDeleted.Count == 0)
                    return;

                // Finish deleting everything
                foreach (double key in softDeleted)
                    StoredMessagesData.Remove(key);
            }
        }
        
        // Log out the amount of memory we're using to store messages
        public void LogQueuedMessagesSize()
        {
            var memUsage = 0;
            if (StoredMessagesData.Count > 0)
            {
                memUsage = StoredMessagesData.Count * StoredMessagesData.First().Value.GetMessageSize() / 1024; // This is in Kilobytes
            }
            LunaLog.Log($"Current memory usage for stored messages in the VesselCouple system: {memUsage}KB");
        }
    }
}
