using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Diagnostics;
using LmpClient.Systems.VesselRemoveSys;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageHandler : SubSystem<VesselProtoSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselProtoMsgData msgData)) return;

            //ARRIVED is recorded BEFORE the kill-list filter so the diagnostic file
            //reflects everything the server actually sent us — including vessels we
            //refused to enqueue — instead of just the subset that survived the
            //first gate. A later DISCARDED line for the same id gives the complete
            //"server sent X, we dropped it because Y" trail for the post-mortem.
            VesselSyncDiagnostics.LogArrived(msgData.VesselId, msgData.NumBytes, msgData.GameTime,
                msgData.ForceReload, msgData.Reason);

            //We don't call VesselCommon.DoVesselChecks(msgData.VesselId) because we may receive a 
            //proto update on our own vessel (when someone docks against us and we don't detect it for example
            //Therefore, we must manually call VesselWillBeKilled and implement only 1 of the checks
            if (VesselRemoveSystem.Singleton.VesselWillBeKilled(msgData.VesselId))
            {
                VesselSyncDiagnostics.LogDiscarded(msgData.VesselId, vesselName: null, parts: -1,
                    reason: "VesselRemoveSystem.VesselWillBeKilled returned true (vessel scheduled for removal)");
                return;
            }

            if (!System.VesselProtos.ContainsKey(msgData.VesselId))
            {
                System.VesselProtos.TryAdd(msgData.VesselId, new VesselProtoQueue());
            }
            if (System.VesselProtos.TryGetValue(msgData.VesselId, out var queue))
            {
                queue.Enqueue(msgData);
            }
        }
    }
}
