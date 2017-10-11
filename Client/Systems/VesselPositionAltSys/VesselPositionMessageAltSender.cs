using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselPositionAltSys
{
    public class VesselPositionMessageAltSender : SubSystem<VesselPositionAltSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPositionUpdate(Vessel vessel)
        {
            var update = new VesselPositionAltUpdate(vessel);
            SendVesselPositionUpdate(update);
        }

        public void SendVesselPositionUpdate(VesselPositionAltUpdate update)
        {
            SendMessage(update.AsSimpleMessage());
        }
    }
}
