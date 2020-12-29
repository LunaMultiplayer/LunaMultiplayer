using HarmonyLib;
using KSP.UI.Screens;
using LmpClient.Systems.Lock;
using LmpClient.Systems.VesselRemoveSys;
using LmpCommon.Enums;
using System.Collections.Generic;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to hide the vessel markers from other controlled vessels in the KSC.
    /// This way we avoid that ppl click on the marker of a vessel that is being controlled by another person.
    /// </summary>
    [HarmonyPatch(typeof(KSCVesselMarkers))]
    [HarmonyPatch("SpawnVesselMarkers")]
    public class KscVesselMarkers_SpawnVesselMarkers
    {
        private static readonly List<KSCVesselMarker> MarkersToRemove = new List<KSCVesselMarker>();

        [HarmonyPostfix]
        private static void PostfixVesselMarkers()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            MarkersToRemove.Clear();

            var markers = Traverse.Create(KSCVesselMarkers.fetch).Field("markers").GetValue<List<KSCVesselMarker>>();
            foreach (var marker in markers)
            {
                var vessel = Traverse.Create(marker).Field("v").GetValue<Vessel>();
                if (LockSystem.LockQuery.ControlLockExists(vessel.id) || VesselRemoveSystem.Singleton.VesselWillBeKilled(vessel.id))
                    MarkersToRemove.Add(marker);
            }

            foreach (var marker in MarkersToRemove)
            {
                markers.Remove(marker);
                marker.Terminate();
            }

            //HACK: Sometimes you can get a lock when holding the mouse over a marker. Here we clear them
            InputLockManager.ClearControlLocks();
        }
    }
}
