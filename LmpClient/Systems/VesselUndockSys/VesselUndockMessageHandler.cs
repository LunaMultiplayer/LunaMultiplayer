using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselUndockSys
{
    public class VesselUndockMessageHandler : SubSystem<VesselUndockSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();
        
        public ConcurrentQueue<VesselUndockMsgData> StoredMessagesData { get; set; } = new ConcurrentQueue<VesselUndockMsgData>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselUndockMsgData msgData)) return;

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
        
        public void TryQueueUpdate(VesselUndockMsgData msgData)
        {
            if (!System.VesselUndocks.ContainsKey(msgData.VesselId))
            {
                System.VesselUndocks.TryAdd(msgData.VesselId, new VesselUndockQueue());
            }

            if (System.VesselUndocks.TryGetValue(msgData.VesselId, out var queue))
            {
                if (queue.TryPeek(out var value) && value.GameTime > msgData.GameTime)
                {
                    //A user reverted, so clear their message queue and start from scratch
                    queue.Clear();
                }

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
            LunaLog.Debug($"Current memory usage for stored messages in the VesselUndock system: {Math.Floor(StoredMessagesData.Count * sizeof(VesselUndockMsgData) / 1024)}KB");
        }
    }
}
