using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            //Nothing done here
        }

        //public void HandleMessage(IMessageData messageData)
        //{
        //    var msgData = messageData as VesselPositionMsgData;

        //    if (msgData == null || !System.PositionUpdateSystemBasicReady || VesselCommon.UpdateIsForOwnVessel(msgData.VesselId))
        //    {
        //        return;
        //    }

        //    var update = new VesselPositionUpdate(msgData);

        //    if (!System.ReceivedUpdates.ContainsKey(update.VesselId))
        //    {
        //        System.ReceivedUpdates.Add(update.VesselId, new Queue<VesselPositionUpdate>());
        //    }

        //    System.ReceivedUpdates[update.VesselId].Enqueue(update);
        //}
    }
}
