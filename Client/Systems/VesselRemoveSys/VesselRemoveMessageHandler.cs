using System;
using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveMessageHandler : SubSystem<VesselRemoveSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselRemoveMsgData;
            if (msgData == null) return;

            RemoveVessel(msgData.VesselId, msgData.IsDockingUpdate, msgData.DockingPlayerName);
        }

        public void RemoveVessel(Guid vesselId, bool isDockingUpdate, string dockingPlayer)
        {
            var vessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == vesselId);

            if (vessel == null) return;

            if (isDockingUpdate)
            {
                if (FlightGlobals.ActiveVessel?.id == vessel.id)
                {
                    var dockingPlayerVessel = FlightGlobals.Vessels
                        .FirstOrDefault(v => LockSystem.Singleton.LockOwner("control-" + v.id) == dockingPlayer);

                    if (dockingPlayerVessel != null)
                    {
                        FlightGlobals.ForceSetActiveVessel(dockingPlayerVessel);
                    }
                    else
                    {
                        HighLogic.LoadScene(GameScenes.TRACKSTATION);
                        ScreenMessages.PostScreenMessage("Kicked to tracking station, a player docked with you but they were not loaded into the game.");
                    }
                }
                Debug.Log($"[LMP]: Removing docked vessel: {vesselId}");
                System.KillVessel(vessel);
            }
            else
            {
                Debug.Log($"[LMP]: Removing vessel: {vesselId}");
                System.KillVessel(vessel);
            }
        }
    }
}
