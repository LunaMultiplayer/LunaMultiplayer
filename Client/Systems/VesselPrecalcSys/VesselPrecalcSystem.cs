namespace LunaClient.Systems.VesselPrecalcSys
{
    /// <summary>
    /// This system packs applis a custom precalc to all vessels so they don't do the kill check if they are not yours
    /// </summary>
    public class VesselPrecalcSystem : Base.System<VesselPrecalcSystem>
    {
        #region Fields & properties

        private VesselPrecalcEvents VesselPrecalcEvents { get; } = new VesselPrecalcEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselPrecalcSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselPrecalcAssign.Add(VesselPrecalcEvents.OnVesselPrecalcAssign);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselPrecalcAssign.Remove(VesselPrecalcEvents.OnVesselPrecalcAssign);
        }

        #endregion
    }
}