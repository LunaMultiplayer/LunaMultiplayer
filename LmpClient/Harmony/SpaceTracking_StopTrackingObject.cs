using Harmony;
using KSP.UI.Screens;
using LmpClient.Events;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when stop tracking an asteroid
    /// </summary>
    [HarmonyPatch(typeof(SpaceTracking))]
    [HarmonyPatch("StopTrackingObject")]
    public class SpaceTracking_StopTrackingObject
    {
        [HarmonyPostfix]
        private static void PostfixStopTrackingObject(Vessel asteroid)
        {
            TrackingEvent.onStopTrackingAsteroid.Fire(asteroid);
        }
    }
}
