using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageSender : SubSystem<VesselRemoveSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        /// <summary>
        /// Sends a vessel remove to the server, it will then broadcast this message to the OTHER clients.
        /// If you set broadcast to true the server will broadcast the msg to ALL the clients.
        /// </summary>
        public void SendVesselRemove(Guid vesselId, bool broadcast = false)
        {
            Debug.Log($"[LMP]: Removing {vesselId} from the server");
            var msg = new VesselRemoveMsgData
            {
                VesselId = vesselId,
                Broadcast = broadcast
            };

            SendMessage(msg);
        }
    }
}