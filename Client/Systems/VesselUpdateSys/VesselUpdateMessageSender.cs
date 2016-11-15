using System;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageSender:SubSystem<VesselUpdateSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselUpdate(VesselPositionUpdate update)
        {
            var msg = update.createVesselUpdateMessage();
            SendMessage(msg);
        }
    }
}
