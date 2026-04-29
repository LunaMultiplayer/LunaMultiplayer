using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselActionGroupSys
{
    public class VesselActionGroupMessageHandler : SubSystem<VesselActionGroupSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();
        
        public ConcurrentQueue<VesselActionGroupMsgData> StoredMessagesData { get; set; } = new ConcurrentQueue<VesselActionGroupMsgData>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselActionGroupMsgData msgData)) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;
            
            // This code helps prevent time paradoxes.
            // Why? Because this code makes sure that we only apply updates that aren't from the future.
            // Additionally, to prevent us from getting desynchronised, we store updates from the future if they change the orbit of the craft, so that we can apply them later.
            // Note that if we stored ALL updates, then the client's RAM would get filled up really quickly with all these updates whenever someone warps a few years into the future.
            var IsFromFuture = VesselCommon.UpdateIsFromFuture(msgData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime);

            if (IsFromFuture)
            {
                StoredMessagesData.Enqueue(msgData);
            }

            TryQueueUpdate(msgData);
        }
        
        public void TryQueueUpdate(VesselActionGroupMsgData msgData)
        {
            if (!System.VesselActionGroups.ContainsKey(msgData.VesselId))
            {
                System.VesselActionGroups.TryAdd(msgData.VesselId, new VesselActionGroupQueue());
            }

            if (System.VesselActionGroups.TryGetValue(msgData.VesselId, out var queue))
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
                if (VesselCommon.UpdateIsFromFuture(msgData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime))
                    StoredMessagesData.Enqueue(StoredMessageData);
                    continue;

                // Apply the update
                TryQueueUpdate(StoredMessageData);
            }
        }
    }
}
