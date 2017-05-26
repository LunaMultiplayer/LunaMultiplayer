using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageHandler : SubSystem<VesselRemoveSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselRemoveMsgData;
            if (msgData == null) return;

            RemoveVessel(msgData.VesselId);
        }

        public void RemoveVessel(Guid vesselId)
        {
            var vessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == vesselId);

            if (vessel != null)
            {
                Debug.Log($"[LMP]: Removing vessel: {vesselId}");
                System.KillVessel(vessel, true);
            }
        }
    }
}
