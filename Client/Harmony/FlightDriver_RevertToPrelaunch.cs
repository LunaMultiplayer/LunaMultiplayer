using Harmony;
using LunaClient.Events;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when reverting to prelaunch (reverting to editor)
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver))]
    [HarmonyPatch("RevertToPrelaunch")]
    public class FlightDriver_RevertToPrelaunch
    {
        [HarmonyPrefix]
        private static void PostfixRevertToPrelaunch(EditorFacility facility)
        {
            RevertEvent.onReturnToEditor.Fire(facility);
        }
    }
}
