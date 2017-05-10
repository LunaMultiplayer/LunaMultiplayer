using System.Collections.Concurrent;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            //Nothing done here
        }

        /// <summary>
        /// This system is particulary heavy in the amount of data it receives so we do the 
        /// queueing in another thread and we just keep the last message received.
        /// </summary>
        public void EnqueueNewMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselPositionMsgData;

            //if (msgData == null || !System.PositionUpdateSystemBasicReady || VesselCommon.UpdateIsForOwnVessel(msgData.VesselId))
            //{
            //    return;
            //}

            if (msgData == null)
            {
                return;
            }

            var update = new VesselPositionUpdate(msgData);

            if (!System.ReceivedUpdates.ContainsKey(update.VesselId))
            {
                System.ReceivedUpdates.TryAdd(update.VesselId, update);
            }
            else
            {
                if (System.ReceivedUpdates[update.VesselId].SentTime < update.SentTime)
                {
                    System.ReceivedUpdates[update.VesselId] = update;
                }
            }
        }
    }
}
