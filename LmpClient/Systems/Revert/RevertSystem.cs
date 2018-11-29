using LmpClient.Base;
using LmpClient.Events;
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
            VesselAssemblyEvent.onAssembledVessel.Add(RevertEvents.VesselAssembled);
            GameEvents.onVesselChange.Add(RevertEvents.OnVesselChange);
            RevertEvent.onRevertedToLaunch.Add(RevertEvents.OnRevertToLaunch);
            GameEvents.onGameSceneLoadRequested.Add(RevertEvents.GameSceneLoadRequested);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselAssemblyEvent.onAssembledVessel.Remove(RevertEvents.VesselAssembled);
            GameEvents.onVesselChange.Remove(RevertEvents.OnVesselChange);
            RevertEvent.onRevertedToLaunch.Remove(RevertEvents.OnRevertToLaunch);
            GameEvents.onGameSceneLoadRequested.Remove(RevertEvents.GameSceneLoadRequested);
        }

        #endregion
    }
}
