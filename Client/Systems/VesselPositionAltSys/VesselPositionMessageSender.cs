using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselPositionAltSys
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
            SendVesselPositionUpdate(update);
        }

        public void SendVesselPositionUpdate(VesselPositionUpdate update)
        {
            SendMessage(update.GetParsedMessage());
        }
    }
}
