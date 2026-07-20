using LmpClient.Extensions;
using System;
using System.Collections.Generic;

namespace LmpClient.VesselUtilities
{
    /// <summary>
    /// Removes protovessels that stock KSP cannot serialize from the current game's flight
    /// state. This happens when a peer created a vessel with mods this client doesn't have:
    /// an orbit around a celestial body that doesn't exist locally (planet pack, e.g. OPM /
    /// Kcalbeloh) or a part whose <c>partInfo</c> won't resolve (part mod, e.g. Restock / KW).
    ///
    /// Stock KSP's <c>Game.Save</c> walks <c>flightState.protoVessels</c> and calls
    /// <c>ProtoVessel.Save</c> on each; an unresolved body or part makes that throw
    /// <see cref="NullReferenceException"/>, which aborts the ENTIRE save and silently stops
    /// the local <c>persistent.sfs</c> from being written. LMP already refuses to LOAD these
    /// vessels (see <see cref="ProtoVesselExtension.Validate"/> /
    /// <see cref="ProtoVesselExtension.HasInvalidParts"/>), but they can still be present in
    /// flight state — most commonly loaded straight from a persistent.sfs that was written in a
    /// previous session while the mod was installed — so we strip them just before any save.
    /// </summary>
    public static class ProtoVesselCleaner
    {
        /// <summary>
        /// Removes every protovessel that stock KSP cannot serialize (unresolved reference body
        /// or missing part) from <c>HighLogic.CurrentGame.flightState.protoVessels</c>. Returns
        /// the number removed. Safe to call at any time: a no-op when there is no current game /
        /// flight state, and it never throws (a failure here must never break the save it is
        /// protecting).
        /// </summary>
        public static int RemoveUnsavableVessels()
        {
            try
            {
                var protoVessels = HighLogic.CurrentGame?.flightState?.protoVessels;
                if (protoVessels == null || protoVessels.Count == 0) return 0;

                List<string> removed = null;
                for (var i = protoVessels.Count - 1; i >= 0; i--)
                {
                    var protoVessel = protoVessels[i];
                    if (protoVessel != null && protoVessel.CanBeSavedByStockGame()) continue;

                    if (removed == null) removed = new List<string>();
                    removed.Add($"{protoVessel?.vesselID.ToString() ?? "<null>"} ({protoVessel?.vesselName ?? "<null>"})");
                    protoVessels.RemoveAt(i);
                }

                if (removed == null) return 0;

                LunaLog.LogWarning($"[LMP]: Removed {removed.Count} protovessel(s) that stock Game.Save cannot " +
                                   $"serialize (missing celestial body or part from a mod this client lacks) before " +
                                   $"save; otherwise ProtoVessel.Save NREs and aborts the entire save. Removed: {string.Join(", ", removed)}");
                return removed.Count;
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: ProtoVesselCleaner.RemoveUnsavableVessels failed: {e}");
                return 0;
            }
        }
    }
}
