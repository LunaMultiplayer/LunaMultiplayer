using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.VesselStore;
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
            var msg = MessageToPositionTransfer.CreateMessageFromVessel(vessel);
            if (msg == null) return;

            msg.GameTime = Planetarium.GetUniversalTime();

            if (!vessel.loaded)
            {
                vessel.orbit?.UpdateFromStateVectors(vessel.orbit.pos, vessel.orbit.vel, vessel.orbit.referenceBody,Planetarium.GetUniversalTime());
            }

            //Update our own values in the store aswell as otherwise if we leave the vessel it can still be in the safety bubble
            VesselsProtoStore.UpdateVesselProtoPosition(msg);

            SendMessage(msg);
        }
    }
}
