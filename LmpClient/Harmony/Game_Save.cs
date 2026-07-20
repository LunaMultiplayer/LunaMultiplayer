using HarmonyLib;
using LmpClient.VesselUtilities;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Stock KSP's <c>Game.Save</c> walks <c>flightState.protoVessels</c> and calls
    /// <c>ProtoVessel.Save</c> on each. A protovessel that references something this client is
    /// missing — an orbit around a celestial body that doesn't exist (planet pack) or a part
    /// whose <c>partInfo</c> won't resolve (part mod) — makes <c>ProtoVessel.Save</c> throw a
    /// <c>NullReferenceException</c>, which aborts the ENTIRE save. The net effect is that the
    /// local <c>persistent.sfs</c> silently stops being written on every autosave/quicksave
    /// while connected.
    ///
    /// LMP already refuses to LOAD those vessels (<see cref="LmpClient.Extensions.ProtoVesselExtension.Validate"/>
    /// / <see cref="LmpClient.Extensions.ProtoVesselExtension.HasInvalidParts"/>), but they can
    /// still be present in flight state, so we strip them just before the save runs. This makes
    /// a mod mismatch degrade gracefully instead of corrupting saves.
    /// </summary>
    [HarmonyPatch(typeof(Game))]
    [HarmonyPatch("Save", new[] { typeof(ConfigNode) })]
    public class Game_Save
    {
        [HarmonyPrefix]
        private static void PrefixSave()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            ProtoVesselCleaner.RemoveUnsavableVessels();
        }
    }
}
