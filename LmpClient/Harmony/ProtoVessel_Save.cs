using HarmonyLib;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;
using System;
using System.Text;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Hardens stock KSP's <c>ProtoVessel.Save</c>, which is called once per vessel from
    /// <c>FlightState.Save</c> during every <c>Game.Save</c> (autosave, quicksave, scene change).
    /// A single vessel that throws here aborts the ENTIRE save, so the local <c>persistent.sfs</c>
    /// silently stops being written for the rest of the session.
    ///
    /// The dominant cause under LMP is a <c>null</c> entry in a part's
    /// <c>protoModuleCrew</c>: the wire format inserts a null placeholder for a <c>crew = NAME</c>
    /// that doesn't resolve in this client's roster (see <see cref="ProtoCrewScrubber"/> /
    /// <see cref="VesselLoader"/>). Those nulls get scrubbed at load time, but the proto stock
    /// actually serialises is regenerated from the live vessel, so the nulls reappear here.
    ///
    /// <list type="number">
    /// <item>Prefix: scrub null crew from the vessel about to be saved. This fixes the common case
    /// cleanly and without data loss — the vessel still saves, just without the unresolved seat.</item>
    /// <item>Finalizer: if the save still throws for any other reason, log exactly which vessel and
    /// which field was null and swallow the exception so the remaining vessels still save. This is
    /// the last-resort guard that keeps one bad vessel from taking down the whole save.</item>
    /// </list>
    /// Both are gated on being connected so stock single-player saves are never altered.
    /// </summary>
    [HarmonyPatch(typeof(ProtoVessel))]
    [HarmonyPatch("Save", new[] { typeof(ConfigNode) })]
    public class ProtoVessel_Save
    {
        [HarmonyPrefix]
        private static void PrefixSave(ProtoVessel __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            try
            {
                var scrub = ProtoCrewScrubber.ScrubNullCrew(__instance);
                if (scrub.Removed > 0)
                {
                    LunaLog.LogWarning($"[LMP]: ProtoVessel.Save prefix scrubbed {scrub.Removed} null protoModuleCrew " +
                                       $"entr{(scrub.Removed == 1 ? "y" : "ies")} across {scrub.PartsAffected} part(s) on " +
                                       $"{__instance?.vesselID} ({__instance?.vesselName}) to prevent stock ProtoVessel.Save " +
                                       $"from NREing and aborting the entire game save.");
                }
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: ProtoVessel.Save crew-scrub prefix failed: {e.Message}");
            }
        }

        [HarmonyFinalizer]
        private static Exception FinalizerSave(Exception __exception, ProtoVessel __instance, ConfigNode node)
        {
            if (__exception == null) return null;
            if (MainSystem.NetworkState < ClientState.Connected) return __exception;

            LunaLog.LogError($"[LMP]: ProtoVessel.Save threw and would have aborted the whole game save; swallowing so the " +
                             $"remaining vessels still persist. Offender: {Describe(__instance)}. Partial node so far: " +
                             $"{SafeCount(() => node?.CountValues)} value(s) / {SafeCount(() => node?.CountNodes)} node(s). " +
                             $"Exception: {__exception}");
            return null;
        }

        private static string SafeCount(Func<int?> getter)
        {
            try { return getter()?.ToString() ?? "?"; }
            catch { return "?"; }
        }

        private static string Describe(ProtoVessel pv)
        {
            try
            {
                if (pv == null) return "<null protovessel>";

                var sb = new StringBuilder();
                sb.Append($"{pv.vesselID} ({pv.vesselName}) situation={pv.situation} type={pv.vesselType}");
                sb.Append($" body={(pv.orbitSnapShot == null ? "no-orbit" : pv.orbitSnapShot.ReferenceBodyIndex.ToString())}");

                var parts = pv.protoPartSnapshots;
                if (parts == null) { sb.Append(" protoPartSnapshots=null"); return sb.ToString(); }

                sb.Append($" parts={parts.Count}");
                for (var i = 0; i < parts.Count; i++)
                {
                    var p = parts[i];
                    if (p == null) { sb.Append($" [#{i}=null-part]"); continue; }
                    if (p.partInfo == null) sb.Append($" [#{i} {p.partName}: partInfo=null]");

                    var crew = p.protoModuleCrew;
                    if (crew != null)
                        for (var c = 0; c < crew.Count; c++)
                            if (crew[c] == null) sb.Append($" [#{i} {p.partName}: crew#{c}=null]");

                    var modules = p.modules;
                    if (modules != null)
                        for (var m = 0; m < modules.Count; m++)
                            if (modules[m] == null) sb.Append($" [#{i} {p.partName}: module#{m}=null]");
                }

                return sb.ToString();
            }
            catch (Exception e)
            {
                return $"<describe failed: {e.Message}>";
            }
        }
    }
}
