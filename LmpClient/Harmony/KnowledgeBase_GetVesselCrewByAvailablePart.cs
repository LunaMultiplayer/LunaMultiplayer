using HarmonyLib;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Strip null entries from every per-part crew list returned by stock
    /// <see cref="KnowledgeBase.GetVesselCrewByAvailablePart"/>, before
    /// <see cref="KbApp_VesselCrew.CreateVesselCrewList"/> hands each list to
    /// <see cref="System.Collections.Generic.List{T}.Sort(System.Comparison{T})"/> with
    /// <see cref="KbApp_VesselCrew.CompareSeatIdx"/> as the comparator.
    ///
    /// Background: stock <see cref="KbApp_VesselCrew.CompareSeatIdx"/> is one line of IL that
    /// dereferences <c>r1.seatIdx</c> and <c>r2.seatIdx</c> directly (no null check), so any null
    /// entry inside the per-part crew list NREs the comparator, which Sort then rethrows as
    /// <c>InvalidOperationException: Failed to compare two elements in the array</c>. The
    /// <c>onPlanetariumTargetChange</c> handler on <see cref="KSP.UI.Screens.KnowledgeBase"/> swallows
    /// the throw, but by that point <see cref="KbApp_VesselCrew.CreateVesselCrewList"/>'s internal
    /// state is half-built and the user is left with a frozen Tracking-Station info pane on the
    /// vessel they just clicked.
    ///
    /// Where the nulls come from: stock <see cref="ProtoPartSnapshot"/>'s ConfigNode constructor
    /// appends a <c>null</c> placeholder to <see cref="ProtoPartSnapshot.protoModuleCrew"/> for every
    /// <c>crew = NAME</c> value whose name doesn't resolve through <c>HighLogic.CurrentGame.CrewRoster</c>
    /// (typical: empty crew name from a peer that lost the name in a wire round-trip, or a tourist
    /// the originating peer has but our roster hasn't received yet). Stock
    /// <see cref="KnowledgeBase.GetVesselCrewByAvailablePart"/> for unloaded vessels (every vessel in
    /// the Tracking Station is unloaded by design) reads <c>v.protoVessel.protoPartSnapshots[*].protoModuleCrew</c>
    /// straight into the returned KVP value with no filtering; <see cref="KbApp_VesselCrew.CreateVesselCrewList"/>
    /// then sorts that list directly. <see cref="LmpClient.VesselUtilities.VesselLoader.ScrubInvalidProtoCrew"/>
    /// already strips these nulls at vessel-load time, but stock KSP can re-introduce them later
    /// through Save+Load round trips driven by autosave (<c>Flight State Captured</c>).
    ///
    /// Why the postfix mutates the result lists in place rather than wrapping them: each KVP value
    /// IS the underlying <see cref="ProtoPartSnapshot.protoModuleCrew"/> reference (or
    /// <see cref="Part.protoModuleCrew"/> for loaded vessels), so a single in-place
    /// <see cref="List{T}.RemoveAll"/> here also cleans every other consumer of the same list —
    /// stock <see cref="KbApp_VesselCrew.SetupKerbalItem"/> downstream of the sort, lab transfers
    /// that walk the same snapshot, and so on.
    ///
    /// Patch is ungated for the same reason as <see cref="Part_RegisterCrew"/>: stripping a stray
    /// null from a crew list is strict-improvement behaviour in single-player too. Wrapped in
    /// try/catch so a postfix-side throw cannot mask the stock return value or break the UI flow.
    /// </summary>
    [HarmonyPatch(typeof(KnowledgeBase))]
    [HarmonyPatch("GetVesselCrewByAvailablePart")]
    public class KnowledgeBase_GetVesselCrewByAvailablePart
    {
        [HarmonyPostfix]
        private static void PostfixGetCrew(Vessel v, List<KeyValuePair<AvailablePart, List<ProtoCrewMember>>> __result)
        {
            if (__result == null) return;
            try
            {
                var totalRemoved = 0;
                var partsAffected = 0;
                for (var i = 0; i < __result.Count; i++)
                {
                    var crew = __result[i].Value;
                    if (crew == null || crew.Count == 0) continue;

                    var removed = crew.RemoveAll(c => c == null);
                    if (removed > 0)
                    {
                        totalRemoved += removed;
                        partsAffected++;
                    }
                }

                if (totalRemoved > 0)
                {
                    LunaLog.LogWarning(
                        $"[LMP]: KnowledgeBase.GetVesselCrewByAvailablePart postfix scrubbed " +
                        $"{totalRemoved} null entr{(totalRemoved == 1 ? "y" : "ies")} across " +
                        $"{partsAffected} part(s) on vessel {v?.id} ({v?.vesselName ?? "<unknown>"}). " +
                        $"Caused by wire-side crew names that don't resolve in this client's CrewRoster — " +
                        $"left in by stock KSP after VesselLoader.ScrubInvalidProtoCrew already cleaned the " +
                        $"proto at load time, typically by a Save+Load round trip during autosave. Without " +
                        $"this scrub stock KSP NREs in KbApp_VesselCrew.CompareSeatIdx (rethrown as " +
                        $"InvalidOperationException: Failed to compare two elements in the array) when the " +
                        $"player focuses this vessel in the Tracking Station, freezing the info pane.");
                }
            }
            catch (Exception e)
            {
                //Defensive cleanup must never break the stock UI flow.
                LunaLog.LogWarning($"[LMP]: KnowledgeBase_GetVesselCrewByAvailablePart postfix failed for vessel {v?.id}: {e.Message}");
            }
        }
    }
}
