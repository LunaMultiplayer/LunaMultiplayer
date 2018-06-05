using LunaClient.Base;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaCommon.Message.Data.Vessel;
using System;
using System.Threading;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class FlightStateQueue : CachedConcurrentQueue<VesselFlightStateUpdate, VesselFlightStateMsgData>
    {
        private const int MaxPacketsInQueue = 5;
        private const float MaxTimeDifference = 1.5f;

        private Guid VesselId { get; set; }

        public FlightStateQueue(Guid vesselId) => SystemBase.LongRunTaskFactory.StartNew(() =>
        {
            VesselId = vesselId;
            while (VesselFlightStateSystem.CurrentFlightState.ContainsKey(VesselId))
            {
                if (Queue.TryPeek(out var outValue))
                {
                    //Only remove old messages that are from future subspaces or from our same subspace!
                    if (WarpSystem.Singleton.SubspaceIdIsMoreAdvancedInTime(outValue.SubspaceId) || WarpSystem.Singleton.CurrentSubspace == outValue.SubspaceId)
                    {
                        if (TimeSyncerSystem.UniversalTime - outValue.GameTimeStamp > MaxTimeDifference)
                        {
                            while (Queue.TryDequeue(out outValue))
                            {
                                if (TimeSyncerSystem.UniversalTime - outValue.GameTimeStamp > MaxTimeDifference)
                                {
                                    Recycle(outValue);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        //This is the case where we received updates from a PAST subspace
                        while (Count > MaxPacketsInQueue && Queue.TryDequeue(out var outExceededValue))
                        {
                            Recycle(outExceededValue);
                        }
                    }
                }
                Thread.Sleep(5);
            }
        });

        protected override void AssignFromMessage(VesselFlightStateUpdate value, VesselFlightStateMsgData msgData)
        {
            value.VesselId = msgData.VesselId;
            value.GameTimeStamp = msgData.GameTime;
            value.SubspaceId = msgData.SubspaceId;

            value.CtrlState.CopyFrom(msgData);
        }
    }
}
