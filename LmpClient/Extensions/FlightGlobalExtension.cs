using System;

namespace LmpClient.Extensions
{
    public static class FlightGlobalExtension
    {
        public static Vessel LmpFindVessel(this FlightGlobals flightGlobals, Guid vesselId)
        {
            return FlightGlobals.FindVessel(vesselId);
        }
    }
}
