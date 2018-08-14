using System;

namespace LunaClient.Extensions
{
    public static class CelestialBodyExtension
    {
        public static double SiderealDayLength(this CelestialBody body)
        {
            if (body == null) return 0;

            var siderealOrbitalPeriod = 6.28318530717959 * Math.Sqrt(Math.Pow(Math.Abs(body.orbit.semiMajorAxis), 3) / body.orbit.referenceBody.gravParameter);
            return body.rotationPeriod * siderealOrbitalPeriod / (siderealOrbitalPeriod + body.rotationPeriod);
        }
    }
}
