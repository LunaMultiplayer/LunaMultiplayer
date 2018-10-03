using Harmony;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger a event after flight driver starts
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver))]
    [HarmonyPatch("Start")]
    public class FlightDriver_Start
    {
        [HarmonyPostfix]
        private static void PostfixStart()
        {
            FlightDriverEvent.onFlightStarted.Fire();
        }
    }
}
