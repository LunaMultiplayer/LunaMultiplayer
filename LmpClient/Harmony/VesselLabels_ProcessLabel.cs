using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when drawing a label
    /// </summary>
    [HarmonyPatch(typeof(VesselLabels))]
    [HarmonyPatch("ProcessLabel")]
    public class VesselLabels_ProcessLabel
    {
        [HarmonyPostfix]
        private static void PostfixProcessLabel(BaseLabel label)
        {
            if (label.gameObject.activeSelf)
            {
                LabelEvent.onLabelProcessed.Fire(label);
            }
        }
    }
}
