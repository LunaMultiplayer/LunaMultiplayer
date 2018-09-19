using Harmony;
using LmpClient.Localization;
using LmpClient.Systems.KerbalSys;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to prevent sacking kerbals if the server does not allow it
    /// </summary>
    [HarmonyPatch(typeof(KerbalRoster))]
    [HarmonyPatch("SackAvailable")]
    public class KerbalRoster_SackAvailable
    {
        [HarmonyPrefix]
        private static bool PrefixSackAvailable(KerbalRoster __instance, ProtoCrewMember ap)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!SettingsSystem.ServerSettings.AllowSackKerbals)
            {
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.SackingKerbalsNotAllowed, 10, ScreenMessageStyle.UPPER_CENTER);
                KerbalSystem.Singleton.RefreshCrewDialog();
                return false;
            }

            return true;
        }
    }
}
