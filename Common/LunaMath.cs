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
        /// Custom unclamped as Unity does not have a lerp for double values
        /// </summary>
        public static double LerpUnclamped(double from, double to, double t)
        {
            return from + (to - from) * t;
        }

        /// <summary>
        /// Custom lerp as Unity does not have a lerp for float values
        /// </summary>
        public static float Lerp(float v0, float v1, float t)
        {
            return (1 - t) * v0 + t * v1;
        }

        /// <summary>
        /// Custom unclamped as Unity does not have a lerp for double values
        /// </summary>
        public static float LerpUnclamped(float from, float to, float t)
        {
            return from + (to - from) * t;
        }

        /// <summary>
        /// Custom lerp as Unity does not have a lerp for bool values
        /// </summary>
        public static bool Lerp(bool v0, bool v1, float t)
        {
            return t < 0.5 ? v0 : v1;
        }

        /// <summary>
        /// Lerps an angle in degrees and wraps around the value specified
        /// </summary>
        public static double LerpAngleDeg(double from, double to, float t, double wrapAngle)
        {
            var angle = LerpAngleDeg(from, to, t);
            if (angle > wrapAngle)
                angle -= 360;
            if (angle <= -wrapAngle)
                angle += 360;
            if (angle <= wrapAngle - 360)
                angle += 360;

            return angle;
        }

        /// <summary>
        /// Lerps an angle in degrees and wraps around the value specified. Returns a value between 0 and wrapAngle
        /// </summary>
        public static double LerpAngleDegAbs(double from, double to, float t, double wrapAngle)
        {
            var angle = LerpAngleDeg(from, to, t, wrapAngle);

            while (angle < 0)
                angle += wrapAngle;

            while (angle > wrapAngle)
                angle -= wrapAngle;

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
        /// Lerps an angle between 0 and 360. Returns a value between 0 and 360
        /// </summary>
        public static double LerpAngleDegAbs(double from, double to, float t)
        {
            var value = LerpAngleDeg(from, to, t);

            while (value < 0)
                value += 360;

            while (value > 360)
                value -= 360;

            return value;
        }

        /// <summary>
        /// Lerps an angle in rad and wraps around the value specified
        /// </summary>
        public static double LerpAngleRad(double from, double to, float t, double wrapAngle)
        {
            var angle = LerpAngleRad(from, to, t);
            if (angle > wrapAngle)
                angle -= 2 * Math.PI;
            if (angle <= -wrapAngle)
                angle += 2 * Math.PI;
            if (angle <= wrapAngle - 2 * Math.PI)
                angle += 2 * Math.PI;

            return angle;
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

        /// <summary>
        /// Clamps a double between min and max
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }
            return value;
        }
        
        private static double Repeat(double t, double length)
        {
            return t - Math.Floor(t / length) * length;
        }
    }
}
