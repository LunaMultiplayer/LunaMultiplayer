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
            if (vessel == null) return;

            var msg = MessageToPositionTransfer.CreateMessageFromVessel(vessel);
            if (msg == null) return;

            msg.GameTime = Planetarium.GetUniversalTime();

            //Update our own values in the store aswell as otherwise if we leave the vessel it can still be in the safety bubble
            VesselsProtoStore.UpdateVesselProtoPosition(msg);
            UpdateOwnVesselProtoFields(vessel, msg.BodyIndex);

            SendMessage(msg);
        }

        private void UpdateOwnVesselProtoFields(Vessel vessel, int bodyIndex)
        {
            if (vessel.protoVessel == null) return;

            if (vessel.protoVessel.orbitSnapShot != null)
            {
                vessel.protoVessel.orbitSnapShot.semiMajorAxis = vessel.orbit.semiMajorAxis;
                vessel.protoVessel.orbitSnapShot.eccentricity = vessel.orbit.eccentricity;
                vessel.protoVessel.orbitSnapShot.inclination = vessel.orbit.inclination;
                vessel.protoVessel.orbitSnapShot.argOfPeriapsis = vessel.orbit.argumentOfPeriapsis;
                vessel.protoVessel.orbitSnapShot.LAN = vessel.orbit.LAN;
                vessel.protoVessel.orbitSnapShot.meanAnomalyAtEpoch = vessel.orbit.meanAnomalyAtEpoch;
                vessel.protoVessel.orbitSnapShot.epoch = vessel.orbit.epoch;
                vessel.protoVessel.orbitSnapShot.ReferenceBodyIndex = bodyIndex;
            }

            vessel.protoVessel.latitude = vessel.latitude;
            vessel.protoVessel.longitude = vessel.longitude;
            vessel.protoVessel.altitude = vessel.altitude;
            vessel.protoVessel.height = vessel.heightFromTerrain;
            vessel.protoVessel.normal = vessel.terrainNormal;
            vessel.protoVessel.rotation = vessel.srfRelRotation;
        }
    }
}
