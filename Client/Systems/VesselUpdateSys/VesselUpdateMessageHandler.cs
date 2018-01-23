using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselUtilities;
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
            
            vessel.vesselName = msgData.Name;
            vessel.vesselType = (VesselType)Enum.Parse(typeof(VesselType), msgData.Type);
            vessel.situation = (Vessel.Situations)Enum.Parse(typeof(Vessel.Situations), msgData.Situation);
            vessel.Landed = msgData.Landed;
            vessel.landedAt = msgData.LandedAt;
            vessel.displaylandedAt = msgData.DisplayLandedAt;
            vessel.Splashed = msgData.Splashed;
            vessel.missionTime = msgData.MissionTime;
            vessel.launchTime = msgData.LaunchTime;
            vessel.lastUT = msgData.LastUt;
            vessel.isPersistent = msgData.Persistent;
            vessel.referenceTransformId = msgData.RefTransformId;

            if (vessel.IsControllable != msgData.Controllable)
            {
                ControllableField.SetValue(vessel, msgData.Controllable);
                vessel.protoVessel.wasControllable = vessel.IsControllable;
            }

            for (var i = 0; i < 17; i++)
            {
                //Ignore SAS if we are spectating as it will fight with the FI
                if ((KSPActionGroup) (1 << (i & 31)) == KSPActionGroup.SAS && VesselCommon.IsSpectating && FlightGlobals.ActiveVessel.id == vessel.id)
                {
                    vessel.ActionGroups.groups[i] = false;
                    continue;
                }

                vessel.ActionGroups.groups[i] = msgData.ActionGroups[i].State;
                vessel.ActionGroups.cooldownTimes[i] = msgData.ActionGroups[i].Time;
            }

            if (vessel.protoVessel != null)
            {
                vessel.protoVessel.vesselName = vessel.name;
                vessel.protoVessel.vesselType = vessel.vesselType;
                vessel.protoVessel.situation = vessel.situation;
                vessel.protoVessel.landed = vessel.Landed;
                vessel.protoVessel.landedAt = vessel.landedAt;
                vessel.protoVessel.displaylandedAt = vessel.displaylandedAt;
                vessel.protoVessel.splashed = vessel.Splashed;
                vessel.protoVessel.missionTime = vessel.missionTime;
                vessel.protoVessel.launchTime = vessel.launchTime;
                vessel.protoVessel.lastUT = vessel.lastUT;
                vessel.protoVessel.persistent = vessel.isPersistent;
                vessel.protoVessel.refTransform = vessel.referenceTransformId;

                if (vessel.IsControllable != msgData.Controllable)
                {
                    vessel.protoVessel.wasControllable = vessel.IsControllable;
                }

                //TODO: Do we need to update the protovessel action group values aswell?
            }
        }
    }
}
