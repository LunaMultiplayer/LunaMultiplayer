using Harmony;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to always assign a persistentID and NEVER use the persistent id of the .craft file
    /// </summary>
    [HarmonyPatch(typeof(ShipConstruct))]
    [HarmonyPatch("LoadShip")]
    [HarmonyPatch(new[] { typeof(ConfigNode), typeof(uint) })]
    public class ShipConstruct_LoadShip
    {
        [HarmonyPrefix]
        private static void PrefixLoadShip(ConfigNode root, ref uint persistentID)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            if (persistentID == 0)
            {
                persistentID = FlightGlobals.GetUniquepersistentId();

                foreach (var part in root.GetNodes("PART"))
                {
                    part.SetValue("persistentId", FlightGlobals.GetUniquepersistentId(), false);
                }
            }
        }
    }
}
