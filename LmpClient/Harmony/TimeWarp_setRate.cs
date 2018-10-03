using Harmony;
using LmpClient.Systems.Warp;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to deny the warping if needed. Adds another layer of security
    /// </summary>
    [HarmonyPatch(typeof(TimeWarp))]
    [HarmonyPatch("SetRate")]
    [HarmonyPatch(new[] { typeof(int), typeof(bool), typeof(bool) })]
    public class TimeWarp_SetRate
    {
        [HarmonyPrefix]
        private static bool PrefixSetRate(ref int rate_index, ref bool instant, ref bool postScreenMessage)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return WarpSystem.Singleton.WarpValidation();
        }
    }
}
