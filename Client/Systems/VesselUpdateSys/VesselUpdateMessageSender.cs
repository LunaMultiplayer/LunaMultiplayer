using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
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
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselUpdateMsgData>();
            msgData.VesselId = vessel.id;
            msgData.Name = vessel.name;
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
            msgData.Controllable = vessel.IsControllable;

            for (var i = 0; i < 17; i++)
            {
                msgData.ActionGroups[i].ActionGroupName = ((KSPActionGroup)(1 << (i & 31))).ToString();
                msgData.ActionGroups[i].State = vessel.ActionGroups.groups[i];
                msgData.ActionGroups[i].Time = vessel.ActionGroups.cooldownTimes[i];
            }
        }
    }
}
