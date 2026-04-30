using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateMessageHandler : SubSystem<VesselFlightStateSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public ConcurrentQueue<VesselFlightStateMsgData> StoredMessagesData { get; set; } = new ConcurrentQueue<VesselFlightStateMsgData>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselFlightStateMsgData msgData)) return;

            var vesselId = msgData.VesselId;
            if (!VesselCommon.DoVesselChecks(vesselId))
                return;

            //System is not ready nor in use so just skip the message
            if (!System.FlightStateSystemReady)
                return;
            
            //We are not close (unpacked range) to this vessel so ignore the message
            if (!System.FlyByWireDictionary.ContainsKey(vesselId))
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
        
        public void TryQueueUpdate(VesselFlightStateMsgData msgData)
        {
            if (!VesselFlightStateSystem.CurrentFlightState.ContainsKey(vesselId))
            {
                VesselFlightStateSystem.CurrentFlightState.TryAdd(vesselId, new VesselFlightStateUpdate(msgData));
                VesselFlightStateSystem.TargetFlightStateQueue.TryAdd(vesselId, new FlightStateQueue());
            }
            else
            {
                VesselFlightStateSystem.TargetFlightStateQueue.TryGetValue(vesselId, out var queue);
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
