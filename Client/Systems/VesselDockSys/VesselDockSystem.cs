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
            GameEvents.onPartUndockComplete.Add(VesselDockEvents.OnPartUndockComplete);
            GameEvents.onPartCoupleComplete.Add(VesselDockEvents.OnPartCoupleComplete);
            GameEvents.onCrewBoardVessel.Add(VesselDockEvents.OnCrewBoard);
            GameEvents.onCrewOnEva.Add(VesselDockEvents.OnCrewEva);
            GameEvents.onCrewTransferred.Add(VesselDockEvents.OnCrewTransfered);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartUndockComplete.Remove(VesselDockEvents.OnPartUndockComplete);
            GameEvents.onPartCoupleComplete.Remove(VesselDockEvents.OnPartCoupleComplete);
            GameEvents.onCrewBoardVessel.Remove(VesselDockEvents.OnCrewBoard);
            GameEvents.onCrewOnEva.Remove(VesselDockEvents.OnCrewEva);
            GameEvents.onCrewTransferred.Remove(VesselDockEvents.OnCrewTransfered);
        }
    }
}
