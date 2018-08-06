using LunaClient.Base;
using LunaClient.Events;

namespace LunaClient.Systems.Label
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
        }

        protected override void OnDisabled()
        {
            LabelEvent.onLabelProcessed.Remove(LabelEvents.OnLabelProcessed);
        }

        #endregion
    }
}
