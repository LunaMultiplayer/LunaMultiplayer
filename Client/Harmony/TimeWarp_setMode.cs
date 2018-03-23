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
    [HarmonyPatch("setMode")]
    public class TimeWarp_SetMode
    {
        [HarmonyPrefix]
        private static bool PrefixSetMode()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return WarpSystem.Singleton.WarpValidation();
        }
    }
}
