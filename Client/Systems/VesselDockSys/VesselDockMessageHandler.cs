using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockMessageHandler : SubSystem<VesselDockSystem>, IMessageHandler
    {
        private static WarpSystem WarpSystem => WarpSystem.Singleton;
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselDockMsgData msgData)) return;

            LunaLog.Log("Docking message received!");

            //Add the new vessel data to the store
            VesselsProtoStore.HandleVesselProtoData(msgData.FinalVesselData, msgData.NumBytes, msgData.DominantVesselId);

            if (FlightGlobals.ActiveVessel?.id == msgData.WeakVesselId)
            {
                LunaLog.Log($"Docking NOT detected. We DON'T OWN the dominant vessel. Switching to {msgData.DominantVesselId}");

                VesselRemoveSystem.Singleton.AddToKillList(FlightGlobals.ActiveVessel.id);
                VesselSwitcherSystem.Singleton.SwitchToVessel(msgData.DominantVesselId);

                WarpSystem.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);
            }
            if (FlightGlobals.ActiveVessel?.id == msgData.DominantVesselId && !VesselCommon.IsSpectating)
            {
                var newProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData, msgData.NumBytes);

                /* This is a strange case were we didn't detect the docking on time and the weak vessel send the new definition...
                 * Usually it happens when a vessel in a future subspace docks with a vessel in the past and the vessel in the past is the dominant vessel
                 * The reason why is bad is because the ModuleDockingNode sent by the WEAK vessel will tell us that we are 
                 * NOT the dominant (because we received the vesselproto from the WEAK vessel) so we won't be able to undock properly...
                 * This will be fixed if we go to the space center and reload again the vessel...
                 */
                LunaLog.Log("Docking NOT detected. We OWN the dominant vessel");

                if (FlightGlobals.FindVessel(msgData.WeakVesselId) != null)
                {
                    LunaLog.Log($"Weak vessel {msgData.WeakVesselId} still exists in our game. Removing it now");
                    VesselRemoveSystem.Singleton.AddToKillList(msgData.WeakVesselId);
                    VesselRemoveSystem.Singleton.KillVessel(msgData.WeakVesselId);
                }

                /* We own the dominant vessel and dind't detected the docking event so we need to reload our OWN vessel
                 * so if we send our own protovessel later, we send the updated definition
                 */
                LunaLog.Log($"Creating the missing parts in our own vessel. Current: {FlightGlobals.ActiveVessel.parts.Count} Expected: {newProto.protoPartSnapshots.Count}");
                //ProtoToVesselRefresh.CreateMissingPartsInCurrentProtoVessel(FlightGlobals.ActiveVessel, newProto);
                VesselLoader.ReloadVessel(newProto);

                LunaLog.Log("Force sending the new proto vessel");
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);

                WarpSystem.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);
                return;
            }

            //Some other 2 players docked so just remove the weak vessel.
            VesselRemoveSystem.Singleton.AddToKillList(msgData.WeakVesselId);
        }
    }
}
