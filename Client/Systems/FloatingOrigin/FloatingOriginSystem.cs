namespace LunaClient.Systems.FloatingOrigin
{
    /// <inheritdoc />
    /// <summary>
    /// This system stores the last offset position so we can position the unpacked vessels correctly
    /// </summary>
    public class FloatingOriginSystem : Base.System<FloatingOriginSystem>
    {
        #region Fields & properties

        public static Vector3d Offset;
        public static Vector3d OffsetNonKrakensbane;

        private static readonly double[] OffsetLatLonAltArray = new double[3];
        private static readonly double[] OffsetNonKrakensbaneArray = new double[3];

        public static double[] OffsetLatLonAlt
        {
            get
            {
                if (FlightGlobals.currentMainBody != null)
                {
                    FlightGlobals.currentMainBody.GetLatLonAlt(Offset, out OffsetLatLonAltArray[0], out OffsetLatLonAltArray[1], out OffsetLatLonAltArray[2]);
                    return OffsetLatLonAltArray;
                }

                return new double[3];
            }
        }
        
        public static double[] OffsetNonKrakensbaneLatLonAlt
        {
            get
            {
                if (FlightGlobals.currentMainBody != null)
                {
                    FlightGlobals.currentMainBody?.GetLatLonAlt(OffsetNonKrakensbane, out OffsetNonKrakensbaneArray[0],
                        out OffsetNonKrakensbaneArray[1], out OffsetNonKrakensbaneArray[2]);

                    return OffsetNonKrakensbaneArray;
                }

                return new double[3];
            }
        }

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
