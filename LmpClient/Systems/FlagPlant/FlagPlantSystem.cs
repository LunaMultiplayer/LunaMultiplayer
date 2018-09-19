namespace LmpClient.Systems.FlagPlant
{
    /// <summary>
    /// This class handles the events when you plant a flag
    /// </summary>
    public class FlagPlantSystem : Base.System<FlagPlantSystem>
    {
        #region Fields & properties
        
        private FlagPlantEvents FlagPlantEvents { get; } = new FlagPlantEvents();
        
        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(FlagPlantSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.afterFlagPlanted.Add(FlagPlantEvents.AfterFlagPlanted);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.afterFlagPlanted.Remove(FlagPlantEvents.AfterFlagPlanted);
        }

        #endregion
    }
}
