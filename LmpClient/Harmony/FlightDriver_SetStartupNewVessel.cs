using Harmony;
using LmpClient.Systems.Mod;
using LmpClient.Windows.BannedParts;
using LmpCommon.Enums;
using System.Linq;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to display the banned parts msg when loading a new vessel
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver))]
    [HarmonyPatch("setStartupNewVessel")]
    public class FlightDriver_SetStartupNewVessel
    {
        [HarmonyPrefix]
        private static bool PrefixSetStartupNewVessel()
        {
            if (MainSystem.NetworkState < ClientState.Connected || string.IsNullOrEmpty(FlightDriver.newShipToLoadPath)) return true;

            var configNode = ConfigNode.Load(FlightDriver.newShipToLoadPath);
            var shipName = configNode.GetValue("ship");
            var partNames = configNode.GetNodes("PART").Select(n => n.GetValue("part").Substring(0, n.GetValue("part").IndexOf('_'))).ToList();

            if (ModSystem.Singleton.ModControl)
            {
                var bannedParts = ModSystem.Singleton.GetBannedPartsFromPartNames(partNames.Distinct()).ToArray();
                if (bannedParts.Any())
                {
                    LunaLog.LogError($"Vessel {shipName} Contains the following banned parts: {string.Join(", ", bannedParts)}");
                    BannedPartsWindow.Singleton.DisplayBannedPartsDialog(shipName, bannedParts);
                    HighLogic.LoadScene(GameScenes.SPACECENTER);
                    return false;
                }
            }

            return true;
        }
    }
}
