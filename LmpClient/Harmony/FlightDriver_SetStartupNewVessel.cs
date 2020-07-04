using Harmony;
using LmpClient.Systems.Mod;
using LmpClient.Systems.SettingsSys;
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
            var resourceNames = configNode.GetNodes("PART").SelectMany(p => p.GetNodes("RESOURCE").Select(r => r.GetValue("name"))).ToList();
            var partCount = configNode.GetNodes("PART").Count();

            if (ModSystem.Singleton.ModControl)
            {
                var bannedParts = ModSystem.Singleton.GetBannedPartsFromPartNames(partNames.Distinct()).ToArray();
                var bannedResources = ModSystem.Singleton.GetBannedResourcesFromResourceNames(resourceNames.Distinct()).ToArray();
                if (bannedParts.Any() || bannedResources.Any())
                {
                    if (bannedParts.Any())
                        LunaLog.LogError($"Vessel {shipName} Contains the following banned parts: {string.Join(", ", bannedParts)}");
                    if (bannedResources.Any())
                        LunaLog.LogError($"Vessel {shipName} Contains the following banned resources: {string.Join(", ", bannedResources)}");

                    BannedPartsResourcesWindow.Singleton.DisplayBannedPartsResourcesDialog(shipName, bannedParts, bannedResources);
                    HighLogic.LoadScene(GameScenes.SPACECENTER);
                    return false;
                }
            }

            if (partCount > SettingsSystem.ServerSettings.MaxVesselParts)
            {
                LunaLog.LogError($"Vessel {shipName} has {partCount} parts and the max allowed in the server is: {SettingsSystem.ServerSettings.MaxVesselParts}");
                BannedPartsResourcesWindow.Singleton.DisplayBannedPartsResourcesDialog(shipName, new string[0], new string[0], partCount);
                HighLogic.LoadScene(GameScenes.SPACECENTER);
                return false;
            }

            return true;
        }
    }
}
