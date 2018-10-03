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
    [HarmonyPatch("setMode")]
    public class TimeWarp_setMode
    {
        [HarmonyPrefix]
        private static bool PrefixSetMode()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return WarpSystem.Singleton.WarpValidation();
        }
    }
}
