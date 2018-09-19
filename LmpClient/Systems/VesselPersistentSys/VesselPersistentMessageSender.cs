using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselPersistentSys
{
    public class VesselPersistentMessageSender : SubSystem<VesselPersistentSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }
        
        public void SendVesselPersistantIdChanged(uint oldVesselPersistentId, uint newVesselPersistentId)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPersistentMsgData>();
            msgData.From = oldVesselPersistentId;
            msgData.To = newVesselPersistentId;
            msgData.PartPersistentChange = false;

            SendMessage(msgData);
        }

        public void SendPartPersistantIdChanged(uint vesselPersistentId, uint oldVesselPersistentId, uint newVesselPersistentId)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselPersistentMsgData>();
            msgData.VesselPersistentId = vesselPersistentId;
            msgData.From = oldVesselPersistentId;
            msgData.To = newVesselPersistentId;
            msgData.PartPersistentChange = true;

            SendMessage(msgData);
        }
    }
}
