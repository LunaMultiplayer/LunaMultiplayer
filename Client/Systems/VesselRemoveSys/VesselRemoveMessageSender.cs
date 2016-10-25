using System;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageSender : SubSystem<VesselRemoveSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselRemove(Guid vesselId)
        {
            Debug.Log($"[LMP]: Removing {vesselId} from the server");
            var msg = new VesselRemoveMsgData
            {
                PlanetTime = Planetarium.GetUniversalTime(),
                VesselId = vesselId,
                IsDockingUpdate = false
            };

            SendMessage(msg);
        }
    }
}
