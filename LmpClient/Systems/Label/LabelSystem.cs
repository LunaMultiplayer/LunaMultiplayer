using LmpClient.Base;
using LmpClient.Events;

namespace LmpClient.Systems.Label
{
    /// <summary>
    /// This class display the player names in the labels
    /// </summary>
    public class LabelSystem : System<LabelSystem>
    {
        private static LabelEvents LabelEvents { get; } = new LabelEvents();

        #region Base overrides

        public override string SystemName { get; } = nameof(LabelSystem);

        protected override void OnEnabled()
        {
            LabelEvent.onLabelProcessed.Add(LabelEvents.OnLabelProcessed);
            LabelEvent.onMapLabelProcessed.Add(LabelEvents.OnMapLabelProcessed);
            LabelEvent.onMapWidgetTextProcessed.Add(LabelEvents.OnMapWidgetTextProcessed);
        }

        protected override void OnDisabled()
        {
            LabelEvent.onLabelProcessed.Remove(LabelEvents.OnLabelProcessed);
            LabelEvent.onMapLabelProcessed.Remove(LabelEvents.OnMapLabelProcessed);
            LabelEvent.onMapWidgetTextProcessed.Remove(LabelEvents.OnMapWidgetTextProcessed);
        }

        #endregion
    }
}
