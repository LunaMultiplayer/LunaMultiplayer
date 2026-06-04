using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Linq;

namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageHandler : SubSystem<VesselProtoSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();
        
        public SortedList<double,VesselProtoMsgData> StoredMessagesData { get; set; } = new SortedList<double,VesselProtoMsgData>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselProtoMsgData msgData)) return;

            //We don't call VesselCommon.DoVesselChecks(msgData.VesselId) because we may receive a 
            //proto update on our own vessel (when someone docks against us and we don't detect it for example
            //Therefore, we must manually call VesselWillBeKilled and implement only 1 of the checks
            if (VesselRemoveSystem.Singleton.VesselWillBeKilled(msgData.VesselId))
                return;
            
            // This code helps prevent time paradoxes.
            // Why? Because this code makes sure that we only apply updates that aren't from the future.
            // Additionally, to prevent us from getting desynchronised, we store updates from the future, so that we can apply them later.
            var IsFromFuture = VesselCommon.UpdateIsFromFuture(msgData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime);

            if (IsFromFuture)
            {
                
                double delta = 0;
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
                return;
            }

            TryQueueUpdate(msgData);
        }
        
        public void TryQueueUpdate(VesselProtoMsgData msgData)
        {
            if (!System.VesselProtos.ContainsKey(msgData.VesselId))
            {
                System.VesselProtos.TryAdd(msgData.VesselId, new VesselProtoQueue());
            }
            if (System.VesselProtos.TryGetValue(msgData.VesselId, out var queue))
            {
                queue.Enqueue(msgData);
            }
        }

        // Search through all stored updates and apply any that are no longer from the future
        public void OnUpdate()
        {
            var softDeleted = new List<double>();
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
        
        // Log out the amount of memory we're using to store messages
        public void LogQueuedMessagesSize()
        {
            var memUsage = 0;
            if (StoredMessagesData.Count > 0)
            {
                memUsage = StoredMessagesData.Count * StoredMessagesData.First().Value.GetMessageSize() / 1024; // This is in Kilobytes
            }
            LunaLog.Log($"Current memory usage for stored messages in the VesselProto system: {memUsage}KB");
        }
    }
}
