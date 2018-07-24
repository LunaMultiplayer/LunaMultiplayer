namespace LunaClient.Systems.FloatingOrigin
{
    /// <summary>
    /// This system stores the last offset position so we can position the unpacked vessels correctly
    /// </summary>
    public class FloatingOriginSystem : Base.System<FloatingOriginSystem>
    {
        #region Fields & properties

        public static Vector3d Offset { get; set; }
        public static double[] OffsetLatLonAlt { get; } = new double[3];
        public static Vector3d OffsetNonKrakensbane { get; set; }
        public static double[] OffsetNonKrakensbaneLatLonAlt { get; } = new double[3];

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
