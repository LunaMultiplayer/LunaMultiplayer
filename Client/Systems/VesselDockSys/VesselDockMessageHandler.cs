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
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselDockMsgData msgData)) return;

            LunaLog.Log("[LMP]: Docking message received!");

            if (FlightGlobals.ActiveVessel?.id == msgData.WeakVesselId)
            {
                LunaLog.Log("[LMP]: Docking NOT detected. We DON'T OWN the dominant vessel");

                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(FlightGlobals.ActiveVessel.id);
                SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(msgData.DominantVesselId);
            }
            if (FlightGlobals.ActiveVessel?.id == msgData.DominantVesselId && !VesselCommon.IsSpectating)
            {
                var newProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData);
                if (VesselCommon.ProtoVesselNeedsToBeReloaded(FlightGlobals.ActiveVessel, newProto))
                {
                    /* This is bad as we didn't detect the docking on time and the weak vessel send the new definition...
                     * The reason why is bad is because the ModuleDockingNode sent by the WEAK vessel will tell us that we are 
                     * NOT the dominant (because we received the vesselproto from the WEAK vessel) so we won't be able to undock properly...
                     * Hopefully this shouldn't happen very often as we gave the dominant vessel 5 seconds to detect it.
                     */
                    LunaLog.Log("[LMP]: Docking NOT detected. We OWN the dominant vessel");

                    /* We own the dominant vessel and dind't detected the docking event so we need to reload our OWN vessel
                     * so if we send our own protovessel later, we send the updated definition
                     */
                    SystemsContainer.Get<VesselProtoSystem>().VesselLoader.ReloadVessel(newProto);
                }
                return;
            }

            //Some other 2 players docked so just remove the weak vessel.
            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(msgData.WeakVesselId);
            SystemsContainer.Get<VesselProtoSystem>().HandleVesselProtoData(msgData.FinalVesselData, msgData.DominantVesselId);
        }
    }
}
