using Harmony;
using LunaClient.Events;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when drawing a label
    /// </summary>
    [HarmonyPatch(typeof(VesselLabels))]
    [HarmonyPatch("ProcessLabel")]
    public class VesselLabels_ProcessLabel
    {
        [HarmonyPostfix]
        private static void PostProcessLabel(BaseLabel label)
        {
            if (label.gameObject.activeSelf)
            {
                LabelEvent.onLabelProcessed.Fire(label);
            }
        }
    }
}
