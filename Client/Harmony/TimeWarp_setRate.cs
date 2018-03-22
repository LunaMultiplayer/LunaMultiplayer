using Harmony;
using LunaClient.Systems.Warp;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to deny the warping if needed. Adds another layer of security
    /// </summary>
    [HarmonyPatch(typeof(TimeWarp))]
    [HarmonyPatch("setRate")]
    [HarmonyPatch(new[] { typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
    public class TimeWarp_SetRate
    {
        [HarmonyPrefix]
        private static bool PrefixSetRate()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return WarpSystem.Singleton.WarpValidation();
        }
    }
}
