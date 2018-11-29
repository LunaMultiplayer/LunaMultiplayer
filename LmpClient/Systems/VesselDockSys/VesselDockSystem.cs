using LmpClient.Base;
using LmpClient.Events;

namespace LmpClient.Systems.VesselDockSys
{
    public class VesselDockSystem : MessageSystem<VesselDockSystem, VesselDockMessageSender, VesselDockMessageHandler>
    {
        private VesselDockEvents VesselDockEvents { get; } = new VesselDockEvents();

        public override string SystemName { get; } = nameof(VesselDockSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            VesselDockEvent.onDocking.Add(VesselDockEvents.OnVesselDocking);
            VesselDockEvent.onDockingComplete.Add(VesselDockEvents.OnDockingComplete);

            GameEvents.onPartUndock.Add(VesselDockEvents.UndockingStart);
            GameEvents.onVesselsUndocking.Add(VesselDockEvents.UndockingComplete);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselDockEvent.onDocking.Remove(VesselDockEvents.OnVesselDocking);
            VesselDockEvent.onDockingComplete.Remove(VesselDockEvents.OnDockingComplete);

            GameEvents.onPartUndock.Remove(VesselDockEvents.UndockingStart);
            GameEvents.onVesselsUndocking.Remove(VesselDockEvents.UndockingComplete);
        }
    }
}
