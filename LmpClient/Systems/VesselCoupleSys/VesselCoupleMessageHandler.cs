using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselCoupleSys
{
    public class VesselCoupleMessageHandler : SubSystem<VesselCoupleSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselCoupleMsgData msgData)) return;

            //We don't call VesselCommon.DoVesselChecks(msgData.VesselId) because we may receive a 
            //proto update on our own vessel (when someone docks against us and we don't detect it for example
            //Therefore, we must manually call VesselWillBeKilled and implement only 1 of the checks
            if (VesselRemoveSystem.Singleton.VesselWillBeKilled(msgData.VesselId))
                return;

            //If the coupling packet affects our active vessel (even if we are spectating) jump to the future subspace
            if (FlightGlobals.ActiveVessel && (FlightGlobals.ActiveVessel.id == msgData.VesselId || FlightGlobals.ActiveVessel.id == msgData.CoupledVesselId))
            {
                LunaLog.Log($"Received a coupling against our own vessel! We own the {(FlightGlobals.ActiveVessel.id == msgData.VesselId ? "Dominant" : "Weak")} vessel");
                WarpSystem.Singleton.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);
            }

            if (!System.VesselCouples.ContainsKey(msgData.VesselId))
            {
                System.VesselCouples.TryAdd(msgData.VesselId, new VesselCoupleQueue());
            }

            if (System.VesselCouples.TryGetValue(msgData.VesselId, out var queue))
            {
                if (queue.TryPeek(out var value) && value.GameTime > msgData.GameTime)
                {
                    //A user reverted, so clear his message queue and start from scratch
                    queue.Clear();
                }

                if (msgData.GameTime <= TimeSyncSystem.UniversalTime)
                {
                    VesselCouple.ProcessCouple(msgData);

                    //Something has gone wrong and we are still with the weak vessel so for this case we need to switch to the dominant vessel and remove our old one...
                    if (FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == msgData.CoupledVesselId)
                    {
                        var vesselToSwitch = FlightGlobals.FindVessel(msgData.VesselId);
                        if (vesselToSwitch != null)
                            FlightGlobals.ForceSetActiveVessel(vesselToSwitch);
                        VesselRemoveSystem.Singleton.KillVessel(msgData.CoupledVesselId, false, "ERROR while docking!");
                    }
                }
                else
                {
                    queue.Enqueue(msgData);
                }
            }
        }
    }
}
