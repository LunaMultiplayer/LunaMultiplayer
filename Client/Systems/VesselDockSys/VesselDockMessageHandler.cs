using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockMessageHandler : SubSystem<VesselDockSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is VesselDockMsgData msgData)) return;

            LunaLog.Log("[LMP]: Docking message received!");

            if (FlightGlobals.ActiveVessel?.id == msgData.WeakVesselId)
            {
                LunaLog.Log("[LMP]: Docking NOT detected. We DON'T OWN the dominant vessel");

                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(FlightGlobals.ActiveVessel, true);
                SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(msgData.DominantVesselId);
            }
            if (FlightGlobals.ActiveVessel?.id == msgData.DominantVesselId && !VesselCommon.IsSpectating)
            {
                var newProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData);

                if (VesselCommon.ProtoVesselNeedsToBeReloaded(FlightGlobals.ActiveVessel.protoVessel, newProto))
                {
                    LunaLog.Log("[LMP]: Docking NOT detected. We OWN the dominant vessel");
                    //We own the dominant vessel and dind't detected the docking event so we need to reload our OWN vessel
                    //so if we send our own protovessel later, we send the updated definition
                    SystemsContainer.Get<VesselProtoSystem>().VesselLoader.ReloadVessel(newProto);
                }
            }

            //Some other 2 players docked so just remove the weak vessel.
            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(FlightGlobals.FindVessel(msgData.WeakVesselId), true);
            SystemsContainer.Get<VesselProtoSystem>().HandleVesselProtoData(msgData.FinalVesselData, msgData.DominantVesselId);
        }
    }
}
