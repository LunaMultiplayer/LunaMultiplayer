using Harmony;
using LunaClient.Systems.ShareStrategy;
using LunaCommon.Enums;
using Strategies;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to disable the check for the strategy activation,
    /// when the ShareStrategySystem is processing incoming messages.
    /// So the strategy can be activated without this check.
    /// </summary>
    [HarmonyPatch(typeof(Strategy))]
    [HarmonyPatch("CanBeActivated")]
    public class Strategy_CanBeActivated
    {
        [HarmonyPrefix]
        private static bool PrefixCanBeActivated()
        {
            if (MainSystem.NetworkState < ClientState.Connected || !ShareStrategySystem.Singleton.Enabled) return true;

            return !ShareStrategySystem.Singleton.IgnoreEvents;
        }
    }
}
