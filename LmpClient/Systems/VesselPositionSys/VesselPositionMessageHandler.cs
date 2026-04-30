using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public ConcurrentQueue<VesselPositionMsgData> StoredMessagesData { get; set; } = new ConcurrentQueue<VesselPositionMsgData>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return;

            var vesselId = msgData.VesselId;
            if (!VesselCommon.DoVesselChecks(vesselId))
                return;
            
            // This code helps prevent time paradoxes.
            // Why? Because this code makes sure that we only apply updates that aren't from the future.
            // Additionally, to prevent us from getting desynchronised, we store updates from the future, so that we can apply them later.
            var IsFromFuture = VesselCommon.UpdateIsFromFuture(msgData.GameTime, WarpSystem.Singleton.CurrentSubspaceTime);

            if (IsFromFuture && VesselCommon.VesselHasSameOrbit(vesselId, msgData.Orbit))
            {
                return;
            }
            else if (IsFromFuture)
            {
                StoredMessagesData.Enqueue(msgData);
                return;
            }

            TryQueueUpdate(msgData)
        }

        public void TryQueueUpdate(VesselPositionMsgData msgData)
        {
            var vesselId = msgData.VesselId;
            if (!VesselPositionSystem.CurrentVesselUpdate.ContainsKey(vesselId))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, new VesselPositionUpdate(msgData));
                VesselPositionSystem.TargetVesselUpdateQueue.TryAdd(vesselId, new PositionUpdateQueue());
            }
            else
            {
                VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(vesselId, out var queue);
                queue?.Enqueue(msgData);
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
