using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselActionGroupSys
{
    public class VesselActionGroupMessageSender : SubSystem<VesselActionGroupSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselActionGroup(Vessel vessel, KSPActionGroup actionGrp, bool value)
        {
            if (vessel == null) return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselActionGroupMsgData>();
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.ActionGroupString = actionGrp.ToString();
            msgData.ActionGroup = (int)actionGrp;
            msgData.Value = value;
            
            SendMessage(msgData);
        }
    }
}
