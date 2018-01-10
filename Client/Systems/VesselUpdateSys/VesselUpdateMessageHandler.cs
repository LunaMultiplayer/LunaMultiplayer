using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageHandler : SubSystem<VesselUpdateSystem>, IMessageHandler
    {
        private static readonly FieldInfo ControllableField = typeof(Vessel).GetField("isControllable", BindingFlags.Instance | BindingFlags.NonPublic);

        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselUpdateMsgData msgData)) return;

            var vessel = FlightGlobals.FindVessel(msgData.VesselId);
            if (vessel == null) return;
            
            vessel.name = msgData.Name;
            vessel.protoVessel.vesselName = vessel.name;

            vessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), msgData.Type);
            vessel.protoVessel.vesselType = vessel.vesselType;
            
            vessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), msgData.Situation);
            vessel.protoVessel.situation = vessel.situation;

            vessel.Landed = msgData.Landed;
            vessel.protoVessel.landed = vessel.Landed;

            vessel.landedAt = msgData.LandedAt;
            vessel.protoVessel.landedAt = vessel.landedAt;

            vessel.displaylandedAt = msgData.DisplayLandedAt;
            vessel.protoVessel.displaylandedAt = vessel.displaylandedAt;

            vessel.Splashed = msgData.Splashed;
            vessel.protoVessel.splashed = vessel.Splashed;

            vessel.missionTime = msgData.MissionTime;
            vessel.protoVessel.missionTime = vessel.missionTime;

            vessel.launchTime = msgData.LaunchTime;
            vessel.protoVessel.launchTime = vessel.launchTime;

            vessel.lastUT = msgData.LastUt;
            vessel.protoVessel.lastUT = vessel.lastUT;

            vessel.isPersistent = msgData.Persistent;
            vessel.protoVessel.persistent = vessel.isPersistent;

            vessel.referenceTransformId = msgData.RefTransformId;
            vessel.protoVessel.refTransform = vessel.referenceTransformId;

            if (vessel.IsControllable != msgData.Controllable)
            {
                ControllableField.SetValue(vessel, msgData.Controllable);
                vessel.protoVessel.wasControllable = vessel.IsControllable;
            }

            for (var i = 0; i < 17; i++)
            {
                vessel.ActionGroups.groups[i] = msgData.ActionGroups[i].State;
                vessel.ActionGroups.cooldownTimes[i] = msgData.ActionGroups[i].Time;
                //TODO: Do we need to update the protovessel values aswell?
            }
        }
    }
}
