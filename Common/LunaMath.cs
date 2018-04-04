using System;

namespace LunaCommon
{
    public class LunaMath
    {
        /// <summary>
        /// Custom lerp as Unity does not have a lerp for double values
        /// </summary>
        public static double Lerp(double from, double to, float t)
        {
            return from * (1 - t) + to * t;
        }

        /// <summary>
        /// Lerps an angle in degrees and wraps around the value specified
        /// </summary>
        public static double LerpAngleDeg(double from, double to, float t, double wrapAngle)
        {
            var retval = from + Repeat(to - from, 360) * t;
            if (retval > wrapAngle)
            {
                retval -= 360f;
            }
            return retval;
        }

        /// <summary>
        /// Lerps an angle between -180 and 180
        /// </summary>
        public static double LerpAngleDeg180(double from, double to, float t)
        {
            var angle = LerpAngleDeg(from, to, t);
            if (angle > 180)
                return angle - 360;
            return angle;
        }

        /// <summary>
        /// Lerps an angle between 0 and 360
        /// </summary>
        public static double LerpAngleDeg(double from, double to, float t)
        {
            var single = Repeat(to - from, 360);
            if (single > 180f)
            {
                single -= 360f;
            }
            return from + single * t;
        }

        /// <summary>
        /// Lerps an angle in rad and wraps around the value specified
        /// </summary>
        public static double LerpAngleRad(double from, double to, float t, double wrapAngle)
        {
            var retval = from + Repeat(to - from, 2 * Math.PI) * t;
            if (retval > wrapAngle)
            {
                retval -= 2 * Math.PI;
            }
            return retval;
        }

        /// <summary>
        /// Lerps an angle between 0 and 2*pi
        /// </summary>
        public static double LerpAngleRad(double from, double to, float t)
        {
            var single = Repeat(to - from, 2 * Math.PI);
            if (single > Math.PI)
            {
                single -= 2 * Math.PI;
            }
            return from + single * t;
        }

        public static double LerpAngleRadPi(double from, double to, float t)
        {
            var angle = LerpAngleRad(from, to, t);
            if (angle > Math.PI)
                return angle - 2 * Math.PI;
            return angle;
        }

        /// <summary>
        /// Clamps a double between 0 and 1
        /// </summary>
        public static double Clamp01(double value)
        {
            double single;
            if (value >= 0f)
            {
                single = value <= 1d ? value : 1d;
            }
            else
            {
                single = 0f;
            }
            return single;
        }

        private static double Repeat(double t, double length)
        {
            return t - Math.Floor(t / length) * length;
        }
    }
}
