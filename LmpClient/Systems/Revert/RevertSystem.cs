using LmpClient.Base;
using System;

namespace LmpClient.Systems.Revert
{
    /// <summary>
    /// This system takes care of all the reverting logic
    /// </summary>
    public class RevertSystem : System<RevertSystem>
    {
        #region Fields & properties

        public static RevertEvents RevertEvents { get; } = new RevertEvents();

        public Guid StartingVesselId { get; set; } = Guid.Empty;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(RevertSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onFlightReady.Add(RevertEvents.FlightReady);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onFlightReady.Remove(RevertEvents.FlightReady);
        }

        #endregion
    }
}
