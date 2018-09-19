using Harmony;
using LmpClient.Systems.ShareStrategy;
using LmpCommon.Enums;
using Strategies;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to disable the check for the strategy deactivation,
    /// when the ShareStrategySystem is processing incoming messages.
    /// So the strategy can be deactivated without this check.
    /// </summary>
    [HarmonyPatch(typeof(Strategy))]
    [HarmonyPatch("CanBeDeactivated")]
    public class Strategy_CanBeDeactivated
    {
        [HarmonyPrefix]
        private static bool PrefixCanBeDeactivated()
        {
            if (MainSystem.NetworkState < ClientState.Connected || !ShareStrategySystem.Singleton.Enabled) return true;

            return !ShareStrategySystem.Singleton.IgnoreEvents;
        }
    }
}
