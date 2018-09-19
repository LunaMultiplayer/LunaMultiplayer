using System;

namespace LmpClient.Extensions
{
    public static class FlightGlobalExtension
    {
        public static Vessel FindVessel(this FlightGlobals flightGlobals, uint persistentVesselId, Guid vesselId)
        {
            return FlightGlobals.FindVessel(persistentVesselId, out var vesselFound) ? vesselFound : 
                FlightGlobals.FindVessel(vesselId);
        }
    }
}
