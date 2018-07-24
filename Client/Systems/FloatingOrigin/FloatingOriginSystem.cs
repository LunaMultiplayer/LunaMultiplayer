namespace LunaClient.Systems.FloatingOrigin
{
    public class FloatingOriginSystem : Base.System<FloatingOriginSystem>
    {
        #region Fields & properties

        public static Vector3d Offset { get; set; }
        public static Vector3d OffsetNonKrakensbane { get; set; }

        private FloatingOriginEvents FloatingOriginEvents { get; } = new FloatingOriginEvents();
        
        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(FloatingOriginSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onFloatingOriginShift.Add(FloatingOriginEvents.FloatingOriginShift);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onFloatingOriginShift.Remove(FloatingOriginEvents.FloatingOriginShift);
        }

        #endregion
    }
}
