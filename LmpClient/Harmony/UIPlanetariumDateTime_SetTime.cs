using HarmonyLib;
using KSP.UI;
using LmpCommon.Enums;
using System;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Suppresses the per-frame OverflowException thrown by KSP's date formatter when the server's game
    /// Universal Time exceeds int.MaxValue (~2.147e9 seconds, roughly 233 Kerbin years).
    /// KSP internally casts the UT double to int inside get_date_from_UT; for very large values that cast
    /// produces int.MinValue, and Math.Abs(int.MinValue) then throws. LMP cannot control the server's
    /// game time, so we silence the crash here and warn once.
    /// </summary>
    [HarmonyPatch(typeof(UIPlanetariumDateTime))]
    [HarmonyPatch("SetTime")]
    public class UIPlanetariumDateTime_SetTime
    {
        private static bool _overflowWarningLogged;

        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception is OverflowException && MainSystem.NetworkState >= ClientState.Connected)
            {
                if (!_overflowWarningLogged)
                {
                    LunaLog.LogWarning("[LMP]: KSP date formatter overflow suppressed: the server's game time " +
                                       $"({Planetarium.GetUniversalTime():F0}s) exceeds int.MaxValue (~233 Kerbin years). " +
                                       "The in-game date display will not update while connected to this server.");
                    _overflowWarningLogged = true;
                }
                return null;
            }

            return __exception;
        }
    }
}
