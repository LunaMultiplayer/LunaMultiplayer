using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselActionGroupSys
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
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.VesselId = vessel.id;
            msgData.ActionGroupString = actionGrp.ToString();
            msgData.ActionGroup = (int)actionGrp;
            msgData.Value = value;

            SendMessage(msgData);
        }
    }
}
