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
            else if (FlightGlobals.ActiveVessel?.id == msgData.DominantVesselId)
            {
                //In case someone docked into us and we dind't detected the event
                SystemsContainer.Get<VesselProtoSystem>().HandleVesselProtoData(msgData.FinalVesselData, msgData.DominantVesselId);

                var newProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData);
                if (FlightGlobals.ActiveVessel.protoVessel.protoPartSnapshots.Count != newProto.protoPartSnapshots.Count)
                {
                    //Someone docked to us and we didn't detect the event so we need to update our own vessel
                    FlightGlobals.ActiveVessel.Unload();
                    newProto.Load(HighLogic.CurrentGame.flightState);
                    FlightGlobals.ForceSetActiveVessel(newProto.vesselRef);
                    //FlightGlobals.ActiveVessel.StartFromBackup(FlightGlobals.ActiveVessel.protoVessel);
                    //FlightGlobals.ActiveVessel.Load();
                }
            }
            else
            {
                //Some other 2 players docked so just remove the weak vessel.
                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(FlightGlobals.FindVessel(msgData.WeakVesselId), true);
                SystemsContainer.Get<VesselProtoSystem>().HandleVesselProtoData(msgData.FinalVesselData, msgData.DominantVesselId);
            }
        }
    }
}
