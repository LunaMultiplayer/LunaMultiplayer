using LunaClient.Base;
using LunaClient.Systems.Warp;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselPositionSys
{
    public class PositionUpdateQueue : CachedConcurrentQueue<VesselPositionUpdate, VesselPositionMsgData>
    {
        private const float MaxTimeDiff = 2f;

        public override bool TryDequeue(out VesselPositionUpdate result)
        {
            return KeepDequeuing(out result);
        }

        private bool KeepDequeuing(out VesselPositionUpdate result)
        {
            var dequeueResult = base.TryDequeue(out result);

            if (dequeueResult)
            {
                if (WarpSystem.Singleton.CurrentlyWarping || WarpSystem.Singleton.WaitingSubspaceIdFromServer || result.SubspaceId == -1) return true;

                if (!WarpSystem.Singleton.SubspaceIdIsMoreAdvancedInTime(result.SubspaceId) && Count > 5)
                {
                    dequeueResult = KeepDequeuing(out result);
                }
                else if (WarpSystem.Singleton.CurrentSubspaceTime - result.GameTimeStamp > MaxTimeDiff)
                {
                    dequeueResult = KeepDequeuing(out result);
                }
            }

            return dequeueResult;
        }

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
