using System;
using LunaClient.Base;
using LunaClient.Systems.VesselProtoSys;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockSystem : System<VesselDockSystem>
    {
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        public override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onPartUndock.Add(VesselDockEvents.OnVesselUndock);
            GameEvents.onPartCouple.Add(VesselDockEvents.OnVesselDock);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartCouple.Remove(VesselDockEvents.OnVesselDock);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }
        
        public void HandleDocking(Guid from, Guid to)
        {
            var fromVessel = FlightGlobals.FindVessel(from);
            var toVessel = FlightGlobals.FindVessel(to);

            var finalVessel = fromVessel != null && toVessel != null
                ? Vessel.GetDominantVessel(fromVessel, toVessel)
                : fromVessel ?? toVessel;
            
            if (finalVessel != null)
            {
                var vesselIdToRemove = finalVessel.id == from ? to : from;
                
                if (finalVessel == FlightGlobals.ActiveVessel)
                {
                    Debug.Log($"[LMP]: Docking: We own the dominant vessel {finalVessel.id}");
                }
                else
                {
                    Debug.Log($"[LMP]: Docking: We DON'T own the dominant vessel {finalVessel.id}. Switching");
                    FlightGlobals.SetActiveVessel(finalVessel);
                }

                var vessel = VesselProtoSystem.Singleton.AllPlayerVessels.FirstOrDefault(v => v.VesselId == vesselIdToRemove);
                if (vessel != null)
                {
                    VesselProtoSystem.Singleton.AllPlayerVessels.Remove(vessel);
                }

                Debug.Log("[LMP]: Docking event over!");
            }
        }
    }
}
