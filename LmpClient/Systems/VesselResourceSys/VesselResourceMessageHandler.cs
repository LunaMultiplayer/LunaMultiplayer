using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;
using System;
using System.Linq;

namespace LmpClient.Systems.VesselResourceSys
{
    public class VesselResourceMessageHandler : SubSystem<VesselResourceSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();
        
        public ConcurrentQueue<VesselResourceMsgData> StoredMessagesData { get; set; } = new ConcurrentQueue<VesselResourceMsgData>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselResourceMsgData msgData)) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;
            
            // This code helps prevent time paradoxes.
            // Why? Because this code makes sure that we only apply updates that aren't from the future.
            // Additionally, to prevent us from getting desynchronised, we store updates from the future, so that we can apply them later.
            var IsFromFuture = VesselCommon.UpdateIsFromFuture(msgData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime);

            if (IsFromFuture)
            {
                StoredMessagesData.Enqueue(msgData);
                return;
            }

            TryQueueUpdate(msgData);
        }
        
        public void TryQueueUpdate(VesselResourceMsgData msgData)
        {
            if (!System.VesselResources.ContainsKey(msgData.VesselId))
            {
                System.VesselResources.TryAdd(msgData.VesselId, new VesselResourceQueue());
            }

            if (System.VesselResources.TryGetValue(msgData.VesselId, out var queue))
            {
                queue.Enqueue(msgData);
            }
        }

        // Search through all stored updates and apply any that are no longer from the future
        public void OnUpdate()
        {
            while (StoredMessagesData.TryDequeue(out var StoredMessageData))
            {
                // Ensure that the update is no longer considered to be from the future
                if (VesselCommon.UpdateIsFromFuture(StoredMessageData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime))
                {
                    StoredMessagesData.Enqueue(StoredMessageData);
                    continue;
                }

                // Apply the update
                TryQueueUpdate(StoredMessageData);
            }
        }
        
        // Log out the amount of memory we're using to store messages
        public void LogQueuedMessagesSize()
        {
            var memUsage = 0;
            if (StoredMessagesData.Count > 0)
            {
                memUsage = StoredMessagesData.Count * StoredMessagesData.First().GetMessageSize() / 1024; // This is in Kilobytes
            }
            LunaLog.Log($"Current memory usage for stored messages in the VesselResource system: {memUsage}KB");
        }
    }
}
