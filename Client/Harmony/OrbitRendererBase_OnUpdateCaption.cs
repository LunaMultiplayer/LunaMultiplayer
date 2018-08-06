using Harmony;
using KSP.UI.Screens.Mapview;
using LunaClient.Events;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when drawing a label from the map view
    /// </summary>
    [HarmonyPatch(typeof(OrbitRendererBase))]
    [HarmonyPatch("objectNode_OnUpdateCaption")]
    public class OrbitRendererBase_OnUpdateCaption
    {
        [HarmonyPostfix]
        private static void PostOnUpdateCaption(OrbitRendererBase __instance, MapNode n, MapNode.CaptionData data)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            LabelEvent.onMapLabelProcessed.Fire(__instance.vessel, data);
        }
    }
}
