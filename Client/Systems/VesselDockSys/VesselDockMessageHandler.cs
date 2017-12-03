using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockMessageHandler : SubSystem<VesselDockSystem>, IMessageHandler
    {
        private static WarpSystem WarpSystem => SystemsContainer.Get<WarpSystem>();
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        private static readonly List<ProtoPartSnapshot> PartsToInit = new List<ProtoPartSnapshot>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselDockMsgData msgData)) return;

            LunaLog.Log("[LMP]: Docking message received!");

            if (FlightGlobals.ActiveVessel?.id == msgData.WeakVesselId)
            {
                LunaLog.Log("[LMP]: Docking NOT detected. We DON'T OWN the dominant vessel");

                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(FlightGlobals.ActiveVessel.id);
                SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(msgData.DominantVesselId);

                WarpSystem.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);
            }
            if (FlightGlobals.ActiveVessel?.id == msgData.DominantVesselId && !VesselCommon.IsSpectating)
            {
                var newProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData);
                if (VesselCommon.ProtoVesselNeedsToBeReloaded(FlightGlobals.ActiveVessel, newProto))
                {
                    /* This is a strange case were we didn't detect the docking on time and the weak vessel send the new definition...
                     * Usually it happens when a vessel in a future subspace docks with a vessel in the past and the vessel in the past is the dominant vessel
                     * The reason why is bad is because the ModuleDockingNode sent by the WEAK vessel will tell us that we are 
                     * NOT the dominant (because we received the vesselproto from the WEAK vessel) so we won't be able to undock properly...
                     * This will be fixed if we go to the space center and reload again the vessel...
                     */
                    LunaLog.Log("[LMP]: Docking NOT detected. We OWN the dominant vessel");

                    /* We own the dominant vessel and dind't detected the docking event so we need to reload our OWN vessel
                     * so if we send our own protovessel later, we send the updated definition
                     */

                    PartsToInit.Clear();
                    FlightGlobals.ActiveVessel.protoVessel = newProto;

                    foreach (var partSnapshot in newProto.protoPartSnapshots)
                    {
                        //Skip parts that already exists
                        if (FlightGlobals.ActiveVessel.parts.Any(p => p.missionID == partSnapshot.missionID && p.craftID == partSnapshot.craftID))
                            continue;

                        var newPart = partSnapshot.Load(FlightGlobals.ActiveVessel, false);
                        FlightGlobals.ActiveVessel.parts.Add(newPart);
                        PartsToInit.Add(partSnapshot);
                    }

                    //Init new parts
                    foreach(var partSnapshot in PartsToInit)
                        partSnapshot.Init(FlightGlobals.ActiveVessel);
                }

                WarpSystem.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);
                return;
            }

            //Some other 2 players docked so just remove the weak vessel.
            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(msgData.WeakVesselId);
            VesselsProtoStore.HandleVesselProtoData(msgData.FinalVesselData, msgData.DominantVesselId);
        }
    }
}
