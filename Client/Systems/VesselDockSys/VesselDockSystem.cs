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
            var fromVessel = FlightGlobals.FindVessel(from);
            var toVessel = FlightGlobals.FindVessel(to);
            Vessel resultVessel;

            //Find the docked craft
            if (fromVessel != null && toVessel != null)
            {
                resultVessel = Vessel.GetDominantVessel(fromVessel, toVessel);
            }
            else
            {
                resultVessel = fromVessel ?? toVessel;
            }
            
            if (resultVessel != null)
            {
                //Update Status if it's us.
                if (resultVessel == FlightGlobals.ActiveVessel)
                {
                    Debug.Log($"[LMP]: Docking: We own the dominant vessel {resultVessel.id}");
                    if (!VesselCommon.IsSpectating)
                    {
                        //Force the control lock off any other player
                        LockSystem.Singleton.AcquireLock("control-" + resultVessel.id, true);
                    }
                }
                else
                {
                    //They docked into us so we must kill our vessel.
                    var oldVesselId = FlightGlobals.ActiveVessel;
                    Debug.Log($"[LMP]: Docking: We DON'T own the dominant vessel {resultVessel.id}. Killing our own old vessel {oldVesselId}");

                    if (!VesselCommon.IsSpectating)
                    {
                        VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(oldVesselId.id);
                    }
                    VesselRemoveSystem.Singleton.KillVessel(FlightGlobals.ActiveVessel, true);

                    //Perhaps instead of kicking to track station we can switch the vessels....
                    //FlightGlobals.ForceSetActiveVessel(resultVessel);
                    HighLogic.LoadScene(GameScenes.TRACKSTATION);
                    ScreenMessages.PostScreenMessage("Kicked to tracking station, a player docked with you.");
                }

                if (DockingMessage != null)
                    DockingMessage.duration = 0f;

                DockingMessage = ScreenMessages.PostScreenMessage("Docked!", 3f, ScreenMessageStyle.UPPER_CENTER);
                Debug.Log("[LMP]: Docking event over!");
            }
        }
    }
}
