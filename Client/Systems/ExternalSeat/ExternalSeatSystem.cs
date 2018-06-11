using LunaClient.Events;

namespace LunaClient.Systems.ExternalSeat
{
    /// <summary>
    /// This system packs applis a custom precalc to all vessels so they don't do the kill check if they are not yours
    /// </summary>
    public class ExternalSeatSystem : Base.System<ExternalSeatSystem>
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
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ExternalSeatEvent.onExternalSeatBoard.Remove(ExternalSeatEvents.ExternalSeatBoard);
            ExternalSeatEvent.onExternalSeatUnboard.Remove(ExternalSeatEvents.ExternalSeatUnboard);
        }

        #endregion
    }
}
