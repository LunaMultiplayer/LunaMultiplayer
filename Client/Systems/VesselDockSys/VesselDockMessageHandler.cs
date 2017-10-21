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
            var msgData = messageData as VesselDockMsgData;
            if (msgData == null) return;

            if (FlightGlobals.ActiveVessel?.id == msgData.WeakVesselId)
            {
                //In case someone docked into us and we dind't detected the event
                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(FlightGlobals.ActiveVessel, true);
                SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(msgData.DominantVesselId);
            }
            if (FlightGlobals.ActiveVessel?.id == msgData.DominantVesselId)
            {
                //In case someone docked into us and we dind't detected the event set our protovessel as the new definition
                //So if we send our own protovessel, we send the new definition
                var newProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData);

                if (VesselCommon.ProtoVesselHasChanges(FlightGlobals.ActiveVessel.protoVessel, newProto))
                {
                    SystemsContainer.Get<VesselProtoSystem>().VesselLoader.ReloadVessel(newProto);
                }
            }

            //Some other 2 players docked so just remove the weak vessel.
            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(FlightGlobals.FindVessel(msgData.WeakVesselId), true);
            SystemsContainer.Get<VesselProtoSystem>().HandleVesselProtoData(msgData.FinalVesselData, msgData.DominantVesselId);
        }
    }
}
