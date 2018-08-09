using Harmony;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to disable pausing while in LMP
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver))]
    [HarmonyPatch("SetPause")]
    public class FlightDriver_SetPause
    {
        [HarmonyPrefix]
        private static bool PrefixSetPause(bool pauseState, bool postScreenMessage)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return !pauseState;
        }
    }
}
