using Harmony;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when we are detaching a part from a vessel into the space
    /// </summary>
    [HarmonyPatch(typeof(EVAConstructionModeEditor))]
    [HarmonyPatch("DropAttachablePart")]
    public class EVAConstructionModeEditor_DropAttachablePart
    {
        [HarmonyPrefix]
        private static void PrefixDropAttachablePart()
        {
            EVAConstructionEvent.onDroppingPart.Fire();
        }

        [HarmonyPostfix]
        private static void PostfixDropAttachablePart()
        {
            EVAConstructionEvent.onDroppedPart.Fire();
        }
    }
}
