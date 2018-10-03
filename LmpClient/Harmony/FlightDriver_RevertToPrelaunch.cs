using Harmony;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when reverting to prelaunch (reverting to editor)
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver))]
    [HarmonyPatch("RevertToPrelaunch")]
    public class FlightDriver_RevertToPrelaunch
    {
        [HarmonyPrefix]
        private static void PrefixRevertToPrelaunch(EditorFacility facility)
        {
            RevertEvent.onReturningToEditor.Fire(facility);
        }

        [HarmonyPostfix]
        private static void PostfixRevertToPrelaunch(EditorFacility facility)
        {
            RevertEvent.onReturnedToEditor.Fire(facility);
        }
    }
}
