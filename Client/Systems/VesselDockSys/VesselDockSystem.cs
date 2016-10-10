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
        private bool _enabled;
        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                    RegisterGameHooks();
                else if (_enabled && !value)
                    UnregisterGameHooks();

                _enabled = value;
            }
        }

        private float LastDockingMessageUpdate { get; set; }
        private ScreenMessage DockingMessage { get; set; }
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        private void RegisterGameHooks()
        {
            GameEvents.onPartCouple.Add(VesselDockEvents.OnVesselDock);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        private void UnregisterGameHooks()
        {
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
                Debug.Log("Sending docked protovessel {dockedVessel.id}");
                
                //Send the new vessel
                VesselProtoSystem.Singleton.CurrentVesselSent = false;

                //Update Status if it's us.
                if (dockedVessel == FlightGlobals.ActiveVessel && LockSystem.Singleton.LockIsOurs("control-" + dockedVessel.id))
                {
                    //Force the control lock off any other player
                    LockSystem.Singleton.AcquireLock("control-" + dockedVessel.id, true);
                }

                if (DockingMessage != null)
                    DockingMessage.duration = 0f;

                DockingMessage = ScreenMessages.PostScreenMessage("Docked!", 3f, ScreenMessageStyle.UPPER_CENTER);
                Debug.Log("Docking event over!");
            }
        }
    }
}
