using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using System;
using System.Collections;
using UnityEngine;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockSystem : Base.System
    {
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onPartUndock.Add(VesselDockEvents.OnVesselUndock);
            GameEvents.onPartCouple.Add(VesselDockEvents.OnVesselDock);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        protected override void OnDisabled()
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

            if (finalVessel != null && toVessel != null)
            {
                var vesselToRemove = finalVessel.id == from ? toVessel : fromVessel;
                var vesselIdToRemove = finalVessel.id == from ? to : from;

                if (finalVessel == FlightGlobals.ActiveVessel)
                {
                    LunaLog.Log($"[LMP]: Docking: We own the dominant vessel {finalVessel.id}");
                }
                else
                {
                    LunaLog.Log($"[LMP]: Docking: We DON'T own the dominant vessel {finalVessel.id}. Switching");
                    SystemsContainer.Get<VesselProtoSystem>().RemoveVesselFromLoadingSystem(finalVessel.id);
                    Client.Singleton.StartCoroutine(SwitchToVessel(finalVessel.id));
                }

                SystemsContainer.Get<VesselRemoveSystem>().MessageSender.SendVesselRemove(vesselIdToRemove, true);
                SystemsContainer.Get<VesselRemoveSystem>().KillVessel(vesselToRemove, true);

                LunaLog.Log("[LMP]: Docking event over!");
            }
        }

        public IEnumerator SwitchToVessel(Guid vesselId)
        {
            yield return new WaitForSeconds(10.5f);
            FlightGlobals.SetActiveVessel(FlightGlobals.FindVessel(vesselId));
        }
    }
}
