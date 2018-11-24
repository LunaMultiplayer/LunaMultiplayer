using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.Lock;
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

            LunaLog.Log(FlightGlobals.ActiveVessel
                ? $"Docking message received! Dominant: {msgData.DominantVesselId} Weak: {msgData.WeakVesselId} Current {FlightGlobals.ActiveVessel.id}"
                : $"Docking message received! Dominant: {msgData.DominantVesselId} Weak: {msgData.WeakVesselId}");

            if (msgData.WeakVesselId == CurrentDockEvent.WeakVesselId && msgData.DominantVesselId == CurrentDockEvent.DominantVesselId && 
                (LunaNetworkTime.UtcNow - CurrentDockEvent.DockingTime) < TimeSpan.FromSeconds(5))
            {
                LunaLog.Log("Docking message received was detected so ignore it");
                return;
            }

            var dominantProto = VesselSerializer.DeserializeVessel(msgData.FinalVesselData, msgData.NumBytes);
            VesselLoader.LoadVessel(dominantProto);

            WarpSystem.WarpIfSubspaceIsMoreAdvanced(msgData.SubspaceId);

            if (FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == msgData.WeakVesselId)
            {
                LunaLog.Log($"Docking NOT detected. We DON'T OWN the dominant vessel. Switching to {msgData.DominantVesselId}");
                if (dominantProto.vesselRef != null)
                {
                    dominantProto.vesselRef.Load();
                    dominantProto.vesselRef.GoOffRails();
                    FlightGlobals.ForceSetActiveVessel(dominantProto.vesselRef);
                }

                LockSystem.Singleton.ReleaseAllVesselLocks(null, msgData.WeakVesselId);
            }
            else if (FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == msgData.DominantVesselId)
            {
                LunaLog.Log("Docking NOT detected. We OWN the dominant vessel");
                foreach (var kerbal in FlightGlobals.ActiveVessel.GetVesselCrew())
                {
                    LockSystem.Singleton.AcquireKerbalLock(kerbal.name, true);
                }
            }

            VesselRemoveSystem.Singleton.KillVessel(msgData.WeakVesselId, true, "Killing weak (active) vessel during a docking that was not detected");
            CurrentDockEvent.Reset();
        }
    }
}
