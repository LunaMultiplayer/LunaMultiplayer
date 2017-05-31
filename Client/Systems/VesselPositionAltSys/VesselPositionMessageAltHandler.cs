using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPositionAltSys
{
    public class VesselPositionMessageAltHandler : SubSystem<VesselPositionAltSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselPositionMsgData;
            if (msgData == null)
            {
                return;
            }

            var update = new VesselPositionAltUpdate(msgData);
            var vesselId = update.VesselId;

            if (!System.CurrentVesselUpdate.TryGetValue(update.VesselId, out var existingPositionUpdate))
            {
                System.CurrentVesselUpdate[vesselId] = update;
                //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                System.UpdatedVesselIds[vesselId] = 0;
            }
            else
            {
                if (existingPositionUpdate.SentTime < update.SentTime)
                {
                    //If there's an existing update, copy the body and vessel objects so they don't have to be looked up later.
                    System.SetBodyAndVesselOnNewUpdate(existingPositionUpdate, update);
                    System.CurrentVesselUpdate[vesselId] = update;

                    //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                    System.UpdatedVesselIds[vesselId] = 0;
                }
            }
        }
    }
}
