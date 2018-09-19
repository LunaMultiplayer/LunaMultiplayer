using LmpClient.Base;

namespace LmpClient.Systems.VesselDockSys
{
    public class VesselDockSystem : MessageSystem<VesselDockSystem, VesselDockMessageSender, VesselDockMessageHandler>
    {
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        public override string SystemName { get; } = nameof(VesselDockSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onPartCouple.Add(VesselDockEvents.OnPartCouple);
            GameEvents.onVesselWasModified.Add(VesselDockEvents.OnVesselWasModified);

            GameEvents.onVesselsUndocking.Add(VesselDockEvents.UndockingComplete);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onPartCouple.Remove(VesselDockEvents.OnPartCouple);
            GameEvents.onVesselWasModified.Remove(VesselDockEvents.OnVesselWasModified);

            GameEvents.onVesselsUndocking.Remove(VesselDockEvents.UndockingComplete);
        }
    }
}
