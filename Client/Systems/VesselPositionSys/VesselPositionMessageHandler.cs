using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is VesselPositionMsgData msgData)) return;

            var update = new VesselPositionUpdate(msgData);
            var vesselId = update.VesselId;

            if (!System.CurrentVesselUpdate.TryGetValue(update.VesselId, out var existingPositionUpdate))
            {
                System.CurrentVesselUpdate[vesselId] = update;
                //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                System.UpdateVesselPositionOnNextFixedUpdate(vesselId);
            }
            else
            {
                if (existingPositionUpdate.SentTime < update.SentTime)
                {
                    //If there's an existing update, copy the body and vessel objects so they don't have to be looked up later.
                    System.SetBodyAndVesselOnNewUpdate(existingPositionUpdate, update);
                    System.CurrentVesselUpdate[vesselId] = update;

                    //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                    System.UpdateVesselPositionOnNextFixedUpdate(vesselId);
                }
            }
        }
    }
}
