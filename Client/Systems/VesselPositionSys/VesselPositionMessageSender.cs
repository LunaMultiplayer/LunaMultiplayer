using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.VesselRemoveSys;
using LunaCommon.Message.Client;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageSender : SubSystem<VesselPositionSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPositionUpdate(Vessel vessel)
        {                
            //TODO: Check if this can be improved as it probably creates a lot of garbage in memory
            var update = new VesselPositionUpdate(vessel);
            TaskFactory.StartNew(() => SendVesselPositionUpdate(update));
        }

        public void SendVesselPositionUpdate(VesselPositionUpdate update)
        {
            if (SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(update.MsgData.VesselId))
                return;

            SendMessage(update.MsgData);
        }
    }
}
