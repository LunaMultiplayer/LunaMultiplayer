using LmpClient.Base;
using LmpClient.Events;

namespace LmpClient.Systems.ExternalSeat
{
    /// <summary>
    /// This system handles the events when a kerbal boards or unboards an external seat
    /// </summary>
    public class ExternalSeatSystem : System<ExternalSeatSystem>
    {
        #region Fields & properties

        private ExternalSeatEvents ExternalSeatEvents { get; } = new ExternalSeatEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ExternalSeatSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            ExternalSeatEvent.onExternalSeatBoard.Add(ExternalSeatEvents.ExternalSeatBoard);
            ExternalSeatEvent.onExternalSeatUnboard.Add(ExternalSeatEvents.ExternalSeatUnboard);
            EvaEvent.onCrewEvaReady.Add(ExternalSeatEvents.CrewEvaReady);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ExternalSeatEvent.onExternalSeatBoard.Remove(ExternalSeatEvents.ExternalSeatBoard);
            ExternalSeatEvent.onExternalSeatUnboard.Remove(ExternalSeatEvents.ExternalSeatUnboard);
            EvaEvent.onCrewEvaReady.Remove(ExternalSeatEvents.CrewEvaReady);
        }

        #endregion
    }
}
