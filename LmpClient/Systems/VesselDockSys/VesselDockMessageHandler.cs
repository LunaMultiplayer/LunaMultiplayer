using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselDockSys
{
    public class VesselDockMessageHandler : SubSystem<VesselDockSystem>, IMessageHandler
    {
        private static WarpSystem WarpSystem => WarpSystem.Singleton;
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselDockMsgData msgData)) return;

            LunaLog.Log("Docking message received!");

            if (msgData.WeakVesselId == CurrentDockEvent.WeakVesselId && msgData.DominantVesselId == CurrentDockEvent.DominantVesselId && 
                (LunaNetworkTime.UtcNow - CurrentDockEvent.DockingTime) < TimeSpan.FromSeconds(5))
            {
                LunaLog.Log("Docking message received was detected so ignore it");
                return;
            }

            if (FlightGlobals.ActiveVessel?.id == msgData.WeakVesselId)
            {
                LunaLog.Log($"Docking NOT detected. We DON'T OWN the dominant vessel. Switching to {msgData.DominantVesselId}");
                WarpSystem.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);

                VesselRemoveSystem.Singleton.KillVessel(FlightGlobals.ActiveVessel, "Killing weak (active) vessel during a docking that was not detected");

                var dominantVessel = FlightGlobals.fetch.FindVessel(msgData.DominantVesselPersistentId, msgData.DominantVesselId);
                if (dominantVessel != null)
                {
                    dominantVessel.Load();
                    dominantVessel.GoOffRails();
                    FlightGlobals.ForceSetActiveVessel(dominantVessel);
                }
                else 
                    HighLogic.LoadScene(GameScenes.SPACECENTER);
            }
            else if (FlightGlobals.ActiveVessel?.id == msgData.DominantVesselId)
            {

                LunaLog.Log("Docking NOT detected. We OWN the dominant vessel");
                WarpSystem.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);

                VesselRemoveSystem.Singleton.KillVessel(FlightGlobals.fetch.FindVessel(msgData.WeakVesselPersistentId, msgData.WeakVesselId), "Weak vessel in a docking");

                var newProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData, msgData.NumBytes);
                VesselLoader.LoadVessel(newProto);

                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, false);
            }
        }
    }
}
