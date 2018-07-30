using System;
using System.Diagnostics.CodeAnalysis;

namespace LunaClient.Systems.FloatingOrigin
{
    /// <summary>
    /// This system stores the last offset position so we can position the unpacked vessels correctly
    /// </summary>
    public class FloatingOriginSystem : Base.System<FloatingOriginSystem>
    {
        #region Fields & properties

        public static Vector3d Offset { get; set; }

        private static double[] _offsetLatLonAlt = new double[3];

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static double[] OffsetLatLonAlt
        {
            get
            {
                if (_offsetLatLonAlt[0] == 0 && _offsetLatLonAlt[1] == 0 && _offsetLatLonAlt[2] == 0 && FlightGlobals.currentMainBody != null)
                {
                    FlightGlobals.currentMainBody.GetLatLonAlt(Offset, out _offsetLatLonAlt[0],
                        out _offsetLatLonAlt[1], out _offsetLatLonAlt[2]);
                }

                return _offsetLatLonAlt;
            }
            set => Array.Copy(value, _offsetLatLonAlt, 3);
        }

        public static Vector3d OffsetNonKrakensbane { get; set; }

        private static double[] _offsetNonKrakensbaneLatLonAlt = new double[3];

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static double[] OffsetNonKrakensbaneLatLonAlt
        {
            get
            {
                if (_offsetNonKrakensbaneLatLonAlt[0] == 0 && _offsetNonKrakensbaneLatLonAlt[1] == 0 && _offsetNonKrakensbaneLatLonAlt[2] == 0 && FlightGlobals.currentMainBody != null)
                {
                    FlightGlobals.currentMainBody.GetLatLonAlt(OffsetNonKrakensbane, out _offsetNonKrakensbaneLatLonAlt[0],
                        out _offsetNonKrakensbaneLatLonAlt[1], out _offsetNonKrakensbaneLatLonAlt[2]);
                }

                return _offsetNonKrakensbaneLatLonAlt;
            }
            set => Array.Copy(value, _offsetNonKrakensbaneLatLonAlt, 3);
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
