using LunaClient.Base;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaCommon.Message.Data.Vessel;
using System;
using System.Threading;

namespace LunaClient.Systems.VesselPositionSys
{
    public class PositionUpdateQueue : CachedConcurrentQueue<VesselPositionUpdate, VesselPositionMsgData>
    {
        private const int MaxPacketsInQueue = 5;
        private const float MaxTimeDifference = 1.5f;

        private Guid VesselId { get; set; }

        public PositionUpdateQueue(Guid vesselId) => SystemBase.LongRunTaskFactory.StartNew(() =>
        {
            VesselId = vesselId;
            while (VesselPositionSystem.CurrentVesselUpdate.ContainsKey(VesselId))
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

        protected override void AssignFromMessage(VesselPositionUpdate value, VesselPositionMsgData msgData)
        {
            value.VesselId = msgData.VesselId;
            value.SubspaceId = msgData.SubspaceId;
            value.BodyIndex = msgData.BodyIndex;
            value.HeightFromTerrain = msgData.HeightFromTerrain;
            value.Landed = msgData.Landed;
            value.Splashed = msgData.Splashed;
            value.GameTimeStamp = msgData.GameTime;
            value.HackingGravity = msgData.HackingGravity;

            Array.Copy(msgData.SrfRelRotation, value.SrfRelRotation, 4);
            Array.Copy(msgData.Velocity, value.Velocity, 3);
            Array.Copy(msgData.LatLonAlt, value.LatLonAlt, 3);
            Array.Copy(msgData.NormalVector, value.NormalVector, 3);
            Array.Copy(msgData.Orbit, value.Orbit, 8);
        }
    }
}
