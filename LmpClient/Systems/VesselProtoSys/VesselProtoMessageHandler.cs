using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.VesselRemoveSys;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageHandler : SubSystem<VesselProtoSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselProtoMsgData msgData)) return;

            //We don't call VesselCommon.DoVesselChecks(msgData.VesselId) because we may receive a 
            //proto update on our own vessel (when someone docks against us and we don't detect it for example
            //Therefore, we must manually call VesselWillBeKilled and implement only 1 of the checks
            if (VesselRemoveSystem.Singleton.VesselWillBeKilled(msgData.VesselId))
                return;

            if (!System.VesselProtos.ContainsKey(msgData.VesselId))
            {
                System.VesselProtos.TryAdd(msgData.VesselId, new VesselProtoQueue());
            }
            if (System.VesselProtos.TryGetValue(msgData.VesselId, out var queue))
            {
                if (queue.TryPeek(out var update) && update.GameTime > msgData.GameTime)
                {
                    //A user reverted, so clear his message queue and start from scratch
                    queue.Clear();
                }

                queue.Enqueue(msgData);
            }
        }
    }
}
