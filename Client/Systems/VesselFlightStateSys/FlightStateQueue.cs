using LunaClient.Base;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class FlightStateQueue : CachedConcurrentQueue<VesselFlightStateUpdate, VesselFlightStateMsgData>
    {
        private const int MaxPacketsInQueue = 5;
        private const float MaxTimeDifference = 1.5f;

        public override bool TryDequeue(out VesselFlightStateUpdate result)
        {
            return KeepDequeuing(out result);
        }

        private bool KeepDequeuing(out VesselFlightStateUpdate result)
        {
            var dequeueResult = base.TryDequeue(out result);

            if (dequeueResult)
            {
                if (!WarpSystem.Singleton.SubspaceIdIsMoreAdvancedInTime(result.SubspaceId) && Count > MaxPacketsInQueue)
                {
                    //This is the case where the message comes from the same subspace or from a subspace in the PAST.
                    //We don't want to have more than 5 packets in the queue so discard the old ones
                    Recycle(result);
                    dequeueResult = KeepDequeuing(out result);
                }
                else if (TimeSyncerSystem.UniversalTime - result.GameTimeStamp > MaxTimeDifference)
                {
                    //This is the case where the message comes from a subspace in the FUTURE.
                    //If the packet is too old, just discard it.
                    Recycle(result);
                    dequeueResult = KeepDequeuing(out result);
                }
            }

            return dequeueResult;
        }

        protected override void AssignFromMessage(VesselFlightStateUpdate value, VesselFlightStateMsgData msgData)
        {
            value.VesselId = msgData.VesselId;
            value.GameTimeStamp = msgData.GameTime;
            value.SubspaceId = msgData.SubspaceId;

            value.CtrlState.CopyFrom(msgData);
        }
    }
}
