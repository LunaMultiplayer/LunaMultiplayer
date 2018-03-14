using LunaClient.Base;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockSystem : MessageSystem<VesselDockSystem, VesselDockMessageSender, VesselDockMessageHandler>
    {
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        public override string SystemName { get; } = nameof(VesselDockSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onPartUndock.Add(VesselDockEvents.OnPartUndock);
            GameEvents.onPartCouple.Add(VesselDockEvents.OnPartCouple);
            GameEvents.onVesselWasModified.Add(VesselDockEvents.OnVesselWasModified);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
            GameEvents.onCrewOnEva.Add(VesselDockEvents.OnCrewEva);
            GameEvents.onCrewTransferred.Add(VesselDockEvents.OnCrewTransfered);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartUndock.Remove(VesselDockEvents.OnPartUndock);
            GameEvents.onPartCouple.Remove(VesselDockEvents.OnPartCouple);
            GameEvents.onVesselWasModified.Remove(VesselDockEvents.OnVesselWasModified);
            GameEvents.onCrewBoardVessel.Remove(VesselDockEvents.OnCrewBoard);
            GameEvents.onCrewOnEva.Remove(VesselDockEvents.OnCrewEva);
            GameEvents.onCrewTransferred.Remove(VesselDockEvents.OnCrewTransfered);
        }
    }
}
