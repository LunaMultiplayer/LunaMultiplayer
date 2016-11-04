using System;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockSystem : System<VesselDockSystem>
    {
        private float LastDockingMessageUpdate { get; set; }
        private ScreenMessage DockingMessage { get; set; }
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        public override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onPartCouple.Add(VesselDockEvents.OnVesselDock);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartCouple.Remove(VesselDockEvents.OnVesselDock);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        public void PrintDockingInProgress()
        {
            if (Time.realtimeSinceStartup - LastDockingMessageUpdate > 1f)
            {
                LastDockingMessageUpdate = Time.realtimeSinceStartup;
                if (DockingMessage != null)
                    DockingMessage.duration = 0f;
                DockingMessage = ScreenMessages.PostScreenMessage("Docking in progress...", 3f,
                    ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public void HandleDocking(Guid from, Guid to)
        {
            //Find the docked craft
            var dockedVessel = FlightGlobals.Vessels.FindLast(v => v.id == from) ??
                               FlightGlobals.Vessels.FindLast(v => v.id == to);

            if ((dockedVessel != null) && !dockedVessel.packed)
            {
                Debug.Log("[LMP]: Sending docked protovessel {dockedVessel.id}");
                
                //Update Status if it's us.
                if (dockedVessel == FlightGlobals.ActiveVessel && LockSystem.Singleton.LockIsOurs("control-" + dockedVessel.id))
                {
                    //Force the control lock off any other player
                    LockSystem.Singleton.AcquireLock("control-" + dockedVessel.id, true);
                }

                if (DockingMessage != null)
                    DockingMessage.duration = 0f;

                DockingMessage = ScreenMessages.PostScreenMessage("Docked!", 3f, ScreenMessageStyle.UPPER_CENTER);
                Debug.Log("[LMP]: Docking event over!");
            }
        }
    }
}
