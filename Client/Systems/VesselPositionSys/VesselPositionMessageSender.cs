using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using UnityEngine;

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
            SendVesselPositionUpdate(update);
        }

        public void SendVesselPositionUpdate(VesselPositionUpdate update)
        {
            SendMessage(new VesselPositionMsgData
            {
                GameSentTime = Time.time,
                PlanetTime = update.PlanetTime,
                VesselId = update.VesselId,
                BodyName = update.BodyName,
                Orbit = update.Orbit,
                LatLonAlt = update.LatLonAlt,
                TransformPosition = update.WorldPosition,
                OrbitPosition = update.OrbitPosition,
                TransformRotation = update.TransformRotation,
                Velocity = update.Velocity,
                OrbitVelocity = update.OrbitVelocity,
                Acceleration = update.Acceleration
            });
        }
    }
}
