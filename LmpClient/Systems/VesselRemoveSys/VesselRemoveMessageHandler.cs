using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;
using System;
using System.Linq;

namespace LmpClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageHandler : SubSystem<VesselRemoveSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();
        
        public ConcurrentQueue<VesselRemoveMsgData> StoredMessagesData { get; set; } = new ConcurrentQueue<VesselRemoveMsgData>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselRemoveMsgData msgData)) return;

            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == msgData.VesselId)
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

            System.KillVessel(msgData.VesselId, msgData.AddToKillList, "Received a vessel remove message from the server");
        }
        
        public void TryQueueUpdate(VesselRemoveMsgData msgData)
        {
            System.KillVessel(msgData.VesselId, msgData.AddToKillList, "Applied a vessel remove message from the server");
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
            LunaLog.Log($"Current memory usage for stored messages in the VesselRemove system: {memUsage}KB");
        }
    }
}
