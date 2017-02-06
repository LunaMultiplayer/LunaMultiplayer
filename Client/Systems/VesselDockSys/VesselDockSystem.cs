using System;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
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
            var resultVessel = FlightGlobals.Vessels.FindLast(v => v.id == from) ??
                               FlightGlobals.Vessels.FindLast(v => v.id == to);

            if ((resultVessel != null) && !resultVessel.packed)
            {
                Debug.Log("[LMP]: Sending docked protovessel {dockedVessel.id}");
                
                //Update Status if it's us.
                if (resultVessel == FlightGlobals.ActiveVessel && !VesselCommon.IsSpectating)
                {
                    //Force the control lock off any other player
                    LockSystem.Singleton.AcquireLock("control-" + resultVessel.id, true);
                }
                else
                {
                    //They docked into us so we must kill our vessel.
                    var oldVesselId = FlightGlobals.ActiveVessel;
                    VesselRemoveSystem.Singleton.KillVessel(FlightGlobals.ActiveVessel, true);

                    //Prehaps instead of kicking to track station we can switch the vessels....
                    //FlightGlobals.ForceSetActiveVessel(resultVessel);
                    HighLogic.LoadScene(GameScenes.TRACKSTATION);
                    ScreenMessages.PostScreenMessage("Kicked to tracking station, a player docked with you.");
                    
                    Debug.Log($"[LMP]: Removing docked vessel: {oldVesselId}");
                }

                if (DockingMessage != null)
                    DockingMessage.duration = 0f;

                DockingMessage = ScreenMessages.PostScreenMessage("Docked!", 3f, ScreenMessageStyle.UPPER_CENTER);
                Debug.Log("[LMP]: Docking event over!");
            }
        }
    }
}
