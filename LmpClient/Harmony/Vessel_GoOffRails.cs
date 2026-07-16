using System.Linq;
using HarmonyLib;
using LmpClient.Events;
using LmpClient.VesselUtilities;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when FINISHED unpacking a vessel.
    /// Also repairs docking port FSM states that fail to restore correctly after vessel loading.
    /// When the standalone DockingPortFix mod is present, LMP defers the FSM repair to it
    /// (the standalone version may contain newer fixes).
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("GoOffRails")]
    public class Vessel_GoOffRails
    {
        private static bool? _standaloneDockFixPresent;

        private static bool StandaloneDockFixPresent
        {
            get
            {
                if (_standaloneDockFixPresent == null)
                    _standaloneDockFixPresent = System.AppDomain.CurrentDomain.GetAssemblies()
                        .Any(a => a.GetName().Name == "DockingPortFix");
                return _standaloneDockFixPresent.Value;
            }
        }

        [HarmonyPostfix]
        private static void PostfixGoOffRails(Vessel __instance)
        {
            RailEvent.onVesselGoneOffRails.Fire(__instance);
            if (!StandaloneDockFixPresent)
                DockingPortUtil.FixDockingPortFsmStates(__instance);
        }
    }
}
