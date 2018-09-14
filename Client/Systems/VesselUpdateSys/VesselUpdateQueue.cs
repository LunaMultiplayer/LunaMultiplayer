using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;
using System;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateQueue : CachedConcurrentQueue<VesselUpdate, VesselUpdateMsgData>
    {
        protected override void AssignFromMessage(VesselUpdate value, VesselUpdateMsgData msgData)
        {
            value.GameTime = msgData.GameTime;
            value.VesselId = msgData.VesselId;
            value.VesselPersistentId = msgData.VesselPersistentId;
            value.Name = msgData.Name.Clone() as string;
            value.Type = msgData.Type.Clone() as string;
            value.DistanceTraveled = msgData.DistanceTraveled;
            value.Situation = msgData.Situation.Clone() as string;
            value.Landed = msgData.Landed;
            value.Splashed = msgData.Splashed;
            value.Persistent = msgData.Persistent;
            value.LandedAt = msgData.LandedAt.Clone() as string;
            value.DisplayLandedAt = msgData.DisplayLandedAt.Clone() as string;
            value.MissionTime = msgData.MissionTime;
            value.LaunchTime = msgData.LaunchTime;
            value.LastUt = msgData.LastUt;
            value.RefTransformId = msgData.RefTransformId;
            value.AutoClean = msgData.AutoClean;
            value.AutoCleanReason = msgData.AutoCleanReason.Clone() as string;
            value.WasControllable = msgData.WasControllable;
            value.Stage = msgData.Stage;
            Array.Copy(msgData.Com, value.Com, 3);
        }
    }
}
