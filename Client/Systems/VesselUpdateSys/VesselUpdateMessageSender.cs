using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
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
            msgData.VesselId = vessel.id;
            msgData.Name = vessel.vesselName;
            msgData.Type = vessel.vesselType.ToString();
            msgData.Situation = vessel.situation.ToString();
            msgData.Landed = vessel.Landed;
            msgData.LandedAt = vessel.landedAtLast;
            msgData.DisplayLandedAt = vessel.displaylandedAt;
            msgData.Splashed = vessel.Splashed;
            msgData.MissionTime = vessel.missionTime;
            msgData.LaunchTime = vessel.launchTime;
            msgData.LastUt = vessel.lastUT;
            msgData.Persistent = vessel.isPersistent;
            msgData.RefTransformId = vessel.referenceTransformId;

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

            SendMessage(msgData);
        }
    }
}
