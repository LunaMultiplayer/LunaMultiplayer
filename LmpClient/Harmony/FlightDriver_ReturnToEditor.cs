using Harmony;
using LmpClient.Events;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when returning to editor
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver))]
    [HarmonyPatch("ReturnToEditor")]
    public class FlightDriver_ReturnToEditor
    {
        [HarmonyPrefix]
        private static void PrefixReturnToEditor(EditorFacility facility)
        {
            RevertEvent.onReturningToEditor.Fire(facility);
        }

        [HarmonyPostfix]
        private static void PostfixReturnToEditor(EditorFacility facility)
        {
            RevertEvent.onReturnedToEditor.Fire(facility);
        }
    }
}
