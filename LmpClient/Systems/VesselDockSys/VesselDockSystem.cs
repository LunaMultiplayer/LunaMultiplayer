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
            GameEvents.onVesselDocking.Add(VesselDockEvents.OnVesselDocking);
            GameEvents.onDockingComplete.Add(VesselDockEvents.OnDockingComplete);

            GameEvents.onVesselsUndocking.Add(VesselDockEvents.UndockingComplete);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselDocking.Remove(VesselDockEvents.OnVesselDocking);
            GameEvents.onDockingComplete.Remove(VesselDockEvents.OnDockingComplete);

            GameEvents.onVesselsUndocking.Remove(VesselDockEvents.UndockingComplete);
        }
    }
}
