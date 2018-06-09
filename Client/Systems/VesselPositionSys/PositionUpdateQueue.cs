using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselPositionSys
{
    public class PositionUpdateQueue : CachedConcurrentQueue<VesselPositionUpdate, VesselPositionMsgData>
    {
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
