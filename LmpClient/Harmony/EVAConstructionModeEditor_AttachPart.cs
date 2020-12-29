using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when just before attaching a part to a vessel
    /// </summary>
    [HarmonyPatch(typeof(EVAConstructionModeEditor))]
    [HarmonyPatch("AttachPart")]
    public class EVAConstructionModeEditor_AttachPart
    {
        [HarmonyPrefix]
        private static void PrefixAttachPart(Part part)
        {
            EVAConstructionEvent.onAttachingPart.Fire(part);
        }
    }
}
