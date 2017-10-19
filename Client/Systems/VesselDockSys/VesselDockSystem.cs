using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;

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
            GameEvents.onVesselWasModified.Add(VesselDockEvents.OnVesselWasModified);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartCouple.Remove(VesselDockEvents.OnVesselDock);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
        }

        public void HandleDocking(VesselDockStructure dock)
        {
            if (dock.DominantVessel == FlightGlobals.ActiveVessel)
            {
                LunaLog.Log($"[LMP]: Docking: We own the dominant vessel {dock.DominantVesselId}");

                FlightGlobals.ActiveVessel.BackupVessel();
                SystemsContainer.Get<VesselProtoSystem>().MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel.protoVessel);
            }
            else
            {
                LunaLog.Log($"[LMP]: Docking: We DON'T own the dominant vessel {dock.DominantVesselId}. Switching");
                SystemsContainer.Get<VesselProtoSystem>().RemoveVesselFromLoadingSystem(dock.DominantVesselId);
                FlightGlobals.ForceSetActiveVessel(dock.DominantVessel);

                SystemsContainer.Get<VesselRemoveSystem>().MessageSender.SendVesselRemove(dock.MinorVesselId, true);
                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.MinorVessel, true);
            }

            LunaLog.Log("[LMP]: Docking event over!");
        }
    }
}
