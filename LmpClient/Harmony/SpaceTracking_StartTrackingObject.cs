using HarmonyLib;
using KSP.UI.Screens;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when start tracking an asteroid
    /// </summary>
    [HarmonyPatch(typeof(SpaceTracking))]
    [HarmonyPatch("StartTrackingObject")]
    public class SpaceTracking_StartTrackingObject
    {
        [HarmonyPostfix]
        private static void PostfixStartTrackingObject(Vessel v)
        {
            TrackingEvent.onStartTrackingAsteroidOrComet.Fire(v);
        }
    }
}
