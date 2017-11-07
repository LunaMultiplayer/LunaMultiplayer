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
            var update = new VesselPositionUpdate(vessel);
            TaskFactory.StartNew(() => SendVesselPositionUpdate(update));
        }

        public void SendVesselPositionUpdate(VesselPositionUpdate update)
        {
            if (SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(update.VesselId))
                return;

            SendMessage(update.AsSimpleMessage());
        }
    }
}
