using HarmonyLib;
using System;
using System.Collections.Generic;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Strip null entries from <see cref="Part.protoModuleCrew"/> before stock <see cref="Part.RegisterCrew"/>
    /// dereferences each entry to call <c>ProtoCrewMember.RegisterExperienceTraits(this)</c> on it.
    ///
    /// Background: when an incoming wire-side vessel proto contains <c>crew = NAME</c> entries whose names
    /// don't resolve through <c>HighLogic.CurrentGame.CrewRoster</c> (typical: empty crew name, or a kerbal
    /// the originating peer has but the server hasn't replicated to us yet), stock KSP's
    /// <c>ProtoPartSnapshot</c> constructor logs <c>"[Protocrewmember]: Instance of crewmember &lt;NAME&gt;
    /// in part X on Y does not exist in the roster"</c> and appends <c>null</c> to
    /// <see cref="ProtoPartSnapshot.protoModuleCrew"/>. <see cref="ProtoPartSnapshot.ConfigurePart"/> then
    /// stores that same list reference into <see cref="Part.protoModuleCrew"/> on scene→FLIGHT, and
    /// <see cref="Part.RegisterCrew"/> NREs out of <c>ProtoVessel.LoadObjects</c> →
    /// <c>FlightDriver.Start</c> the moment the player tries to fly the vessel — leaving them on a black
    /// flight scene. The half-instantiated vessel then NREs every FixedUpdate forever in
    /// <c>ModuleCommand.UpdateControlSourceState</c> on top of that.
    ///
    /// <see cref="LmpClient.VesselUtilities.VesselLoader.ScrubInvalidProtoCrew"/> already strips these
    /// nulls at vessel-load time, but stock KSP can re-introduce them later through paths that round-trip
    /// the proto through a ConfigNode (notably <c>Game.Save</c> → <c>Flight State Captured</c> autosave,
    /// which deserialises <c>ProtoPartSnapshot</c> from the just-saved tree on certain branches and
    /// re-resolves the still-missing crew names exactly the same way). Any of those re-introduced nulls
    /// reach <see cref="Part.RegisterCrew"/> here. Scrubbing in this prefix is the last line of defense
    /// before the NRE: by the time stock walks the list it has already been mutated in place, so the for
    /// loop sees only real <see cref="ProtoCrewMember"/> entries.
    ///
    /// Mutating <see cref="Part.protoModuleCrew"/> in place also cleans
    /// <see cref="ProtoPartSnapshot.protoModuleCrew"/> on the originating snapshot, because stock
    /// <see cref="ProtoPartSnapshot.ConfigurePart"/> assigns the snapshot's list reference directly
    /// (<c>part.protoModuleCrew = this.protoModuleCrew</c>). One scrub here therefore also keeps
    /// downstream non-flight readers safe — Tracking-Station UI iteration, lab transfer dialogs, and
    /// any other code that walks the snapshot for crew enumeration.
    ///
    /// The patch is intentionally ungated: scrubbing genuine nulls from a crew list is benign in
    /// single-player too (stock KSP itself never expects to see them past this point — every other
    /// stock consumer dereferences the entries without a null check), so leaving the patch active even
    /// when <see cref="LmpClient.MainSystem.NetworkState"/> is below <c>Connected</c> just hardens
    /// against the same shape if it ever appears from a corrupted local save. Wrapped in try/catch so
    /// a broken list (e.g., concurrent mutation by another mod) cannot prevent the stock body from
    /// running.
    /// </summary>
    [HarmonyPatch(typeof(Part))]
    [HarmonyPatch("RegisterCrew")]
    public class Part_RegisterCrew
    {
        [HarmonyPrefix]
        private static void PrefixRegisterCrew(Part __instance)
        {
            try
            {
                var crew = __instance?.protoModuleCrew;
                if (crew == null || crew.Count == 0) return;

                var removed = crew.RemoveAll(c => c == null);
                if (removed > 0)
                {
                    LunaLog.LogWarning(
                        $"[LMP]: Part.RegisterCrew prefix scrubbed {removed} null protoModuleCrew " +
                        $"entr{(removed == 1 ? "y" : "ies")} on part {__instance.partInfo?.name ?? "<unknown>"} " +
                        $"({__instance.flightID}) of vessel {__instance.vessel?.id} " +
                        $"({__instance.vessel?.vesselName ?? "<unknown>"}). " +
                        $"Caused by wire-side crew names that don't resolve in this client's CrewRoster — " +
                        $"left in by stock KSP after VesselLoader.ScrubInvalidProtoCrew already cleaned the " +
                        $"proto at load time, typically by a Save+Load round trip during autosave. Without " +
                        $"this scrub stock KSP NREs in Part.RegisterCrew here and again every FixedUpdate " +
                        $"in ModuleCommand.UpdateControlSourceState.");
                }
            }
            catch (Exception e)
            {
                //Defensive cleanup must never break the stock body: returning normally lets the loop run.
                LunaLog.LogWarning($"[LMP]: Part_RegisterCrew prefix failed for part {__instance?.flightID}: {e.Message}");
            }
        }
    }
}
