using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageSender : SubSystem<VesselUpdateSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselUpdate(Vessel vessel)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselUpdateMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.Name = vessel.vesselName;
            msgData.Type = vessel.vesselType.ToString();
            msgData.DistanceTraveled = vessel.distanceTraveled;

            msgData.Situation = vessel.situation.ToString();
            msgData.Landed = vessel.Landed;
            msgData.LandedAt = vessel.landedAt;
            msgData.DisplayLandedAt = vessel.displaylandedAt;
            msgData.Splashed = vessel.Splashed;
            msgData.MissionTime = vessel.missionTime;
            msgData.LaunchTime = vessel.launchTime;
            msgData.LastUt = vessel.lastUT;
            msgData.Persistent = vessel.isPersistent;
            msgData.RefTransformId = vessel.referenceTransformId;

            msgData.AutoClean = vessel.AutoClean;
            msgData.AutoCleanReason = vessel.AutoCleanReason;
            msgData.WasControllable = vessel.IsControllable;
            msgData.Stage = vessel.currentStage;
            msgData.Com[0] = vessel.localCoM.x;
            msgData.Com[1] = vessel.localCoM.y;
            msgData.Com[2] = vessel.localCoM.z;

            vessel.protoVessel.autoClean = vessel.AutoClean;
            vessel.protoVessel.autoCleanReason = vessel.AutoCleanReason;
            vessel.protoVessel.wasControllable = vessel.IsControllable;
            vessel.protoVessel.CoM = vessel.localCoM;
            vessel.protoVessel.stage = vessel.currentStage;

            for (var i = 0; i < 17; i++)
            {
                if (msgData.ActionGroups[i] == null)
                    msgData.ActionGroups[i] = new ActionGroup();

                msgData.ActionGroups[i].ActionGroupName = ((KSPActionGroup)(1 << (i & 31))).ToString();
                msgData.ActionGroups[i].State = vessel.ActionGroups.groups[i];
                msgData.ActionGroups[i].Time = vessel.ActionGroups.cooldownTimes[i];
            }

            //Update our own values in the store aswell as otherwise if we leave the vessel it can still be in the safety bubble
            VesselsProtoStore.UpdateVesselProtoValues(msgData);
            UpdateOwnProtoVesselValues(vessel);

            SendMessage(msgData);
        }

        private static void UpdateOwnProtoVesselValues(Vessel vessel)
        {
            if (vessel.protoVessel == null) return;

            vessel.protoVessel.vesselName = vessel.vesselName;
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

            vessel.ActionGroups.Save(vessel.protoVessel.actionGroups);
        }
    }
}
