using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.VesselRemoveSys;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageHandler : SubSystem<VesselProtoSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();
        
        public ConcurrentQueue<VesselProtoMsgData> StoredMessagesData { get; set; } = new ConcurrentQueue<VesselProtoMsgData>();

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
                StoredMessagesData.Enqueue(msgData);
                return;
            }

            TryQueueUpdate(msgData);
        }
        
        public void TryQueueUpdate(VesselPartSyncFieldMsgData msgData)
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
            while (StoredMessagesData.TryDequeue(out var StoredMessageData))
            {
                // Ensure that the update is no longer considered to be from the future
                if (VesselCommon.UpdateIsFromFuture(StoredMessageData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime))
                    StoredMessagesData.Enqueue(StoredMessageData);
                    continue;

                // Apply the update
                TryQueueUpdate(StoredMessageData);
            }
        }
    }
}
