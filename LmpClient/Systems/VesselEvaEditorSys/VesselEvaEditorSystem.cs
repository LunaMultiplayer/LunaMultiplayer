using LmpClient.Base;
using LmpClient.Events;

namespace LmpClient.Systems.VesselEvaEditorSys
{
    /// <summary>
    /// This system handles the vessel updates when a kerbal in EVA add or remove parts to a vessel.
    /// </summary>
    public class VesselEvaEditorSystem : System<VesselEvaEditorSystem>
    {
        #region Fields & properties

        public VesselEvaEditorEvents VesselEvaEditorEventsEvents { get; } = new VesselEvaEditorEvents();

        public bool DetachingPart { get; set; }

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselEvaEditorSystem);


        protected override void OnEnabled()
        {
            base.OnEnabled();

            GameEvents.onNewVesselCreated.Add(VesselEvaEditorEventsEvents.VesselCreated);

            EVAConstructionEvent.onAttachingPart.Add(VesselEvaEditorEventsEvents.OnAttachingPart);
            EVAConstructionEvent.onDroppingPart.Add(VesselEvaEditorEventsEvents.OnDroppingPart);
            EVAConstructionEvent.onDroppedPart.Add(VesselEvaEditorEventsEvents.OnDroppedPart);

            GameEvents.OnEVAConstructionModePartAttached.Add(VesselEvaEditorEventsEvents.EVAConstructionModePartAttached);
            GameEvents.OnEVAConstructionModePartDetached.Add(VesselEvaEditorEventsEvents.EVAConstructionModePartDetached);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            GameEvents.onNewVesselCreated.Remove(VesselEvaEditorEventsEvents.VesselCreated);

            EVAConstructionEvent.onDroppingPart.Remove(VesselEvaEditorEventsEvents.OnDroppingPart);
            EVAConstructionEvent.onDroppedPart.Remove(VesselEvaEditorEventsEvents.OnDroppedPart);

            GameEvents.OnEVAConstructionModePartAttached.Remove(VesselEvaEditorEventsEvents.EVAConstructionModePartAttached);
            GameEvents.OnEVAConstructionModePartDetached.Remove(VesselEvaEditorEventsEvents.EVAConstructionModePartDetached);
            DetachingPart = false;
        }

        #endregion
    }
}
