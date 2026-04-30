using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselCoupleSys
{
    public class VesselCoupleMessageHandler : SubSystem<VesselCoupleSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselCoupleMsgData msgData)) return;

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

            var affectsActiveVessel = FlightGlobals.ActiveVessel && (FlightGlobals.ActiveVessel.id == msgData.VesselId || FlightGlobals.ActiveVessel.id == msgData.CoupledVesselId);

            //If the coupling packet affects our active vessel (even if we are spectating) jump to the future subspace
            // Note: This code may not play nicely with the time paradoxes fix.
            // If something breaks with this system when players warp, then this code is probably the cause. Try moving the time paradoxes fix below this conditional.
            if (affectsActiveVessel)
            {
                LunaLog.Log($"Received a coupling against our own vessel! We own the {(FlightGlobals.ActiveVessel.id == msgData.VesselId ? "Dominant" : "Weak")} vessel");
                WarpSystem.Singleton.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);
            }
            
            TryQueueUpdate(msgData);
        }
        
        public void TryQueueUpdate(VesselPartSyncFieldMsgData msgData)
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
    }
}
