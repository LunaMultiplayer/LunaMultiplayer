using LmpClient.Base;
using LmpClient.Events;

namespace LmpClient.Systems.VesselCrewSys
{
    /// <summary>
    /// System for handling when a kerbal boards/unboards a vessel or gets transfered
    /// </summary>
    public class VesselCrewSystem : System<VesselCrewSystem>
    {
        #region Fields & properties

        private VesselCrewEvents VesselCrewEvents { get; } = new VesselCrewEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselCrewSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onCrewOnEva.Add(VesselCrewEvents.OnCrewEva);
            GameEvents.onVesselCrewWasModified.Add(VesselCrewEvents.OnCrewModified);
            EvaEvent.onCrewEvaReady.Add(VesselCrewEvents.CrewEvaReady);
            EvaEvent.onCrewEvaBoarded.Add(VesselCrewEvents.OnCrewBoard);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onCrewOnEva.Remove(VesselCrewEvents.OnCrewEva);
            GameEvents.onVesselCrewWasModified.Remove(VesselCrewEvents.OnCrewModified);
            EvaEvent.onCrewEvaReady.Remove(VesselCrewEvents.CrewEvaReady);
            EvaEvent.onCrewEvaBoarded.Remove(VesselCrewEvents.OnCrewBoard);
        }

        #endregion
    }
}
