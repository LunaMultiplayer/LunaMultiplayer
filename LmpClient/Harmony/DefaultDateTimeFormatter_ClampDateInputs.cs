using HarmonyLib;
using System;
using System.Reflection;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Defends KSP's date formatter against absurdly-large universal times.
    ///
    /// <para>
    /// <see cref="KSPUtil.DefaultDateTimeFormatter"/>.<c>get_date_from_UT(double time, int year_len, int day_len)</c>
    /// internally casts <c>time / year_len</c> to <see cref="int"/> and then calls <see cref="UnityEngine.Mathf.Abs(int)"/>
    /// on the result. When <c>|time / year_len|</c> exceeds <see cref="int.MaxValue"/>, Mono's <c>conv.i4</c> on the
    /// out-of-range double yields <see cref="int.MinValue"/>, and <c>Mathf.Abs(int.MinValue)</c> throws
    /// <see cref="OverflowException"/> ("Negating the minimum value of a twos complement number is invalid"). For the
    /// stock Kerbin year (~9.2e6 s) the failure threshold is around 1.97e16 seconds; for the Earth-time formatter
    /// (year_len = 31_536_000 s) it is around 6.77e16 seconds. Either threshold is reachable when a player or griefer
    /// has warped the server clock forward by an extreme amount - in the wild we have observed subspace times near
    /// 7.5e18 s.
    /// </para>
    ///
    /// <para>
    /// Two callers are affected on the client:
    /// <list type="bullet">
    ///   <item><description><c>KSP.UI.UIPlanetariumDateTime.Update</c> via <see cref="KSPUtil.PrintDate(double, bool, bool)"/>
    ///     - spams the log every frame in any scene that draws the planetarium date widget.</description></item>
    ///   <item><description><c>LmpClient.Windows.Status.StatusTexts.GetTimeLabel</c> via <see cref="KSPUtil.PrintDateCompact(double, bool, bool)"/>
    ///     - blows up the status window when a remote subspace time exceeds the threshold, even if the local
    ///     player's clock has not advanced that far. The thrown exception escapes the GUI window draw and trips
    ///     "GUI Error: You are pushing more GUIClips than you are popping".</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// All three patches below are registered manually via <see cref="HarmonyLib.Harmony.Patch"/> rather than by
    /// attribute, for two reasons that apply to every target:
    /// <list type="number">
    ///   <item><description>The targets are non-public helpers on <see cref="KSPUtil.DefaultDateTimeFormatter"/>
    ///     (visible only in stack traces / via reflection) and may differ across KSP builds. Manual registration
    ///     wrapped in try/catch silently skips any target that is absent on the active build, so a stripped or
    ///     re-shaped DLL can never bring down LMP startup. Attribute-driven <c>PatchAll</c> would instead throw.</description></item>
    ///   <item><description>Harmony's attribute-driven prefix binds <c>ref</c> parameters by name, which silently
    ///     degrades if the shipped assembly does not carry parameter names; positional <c>__0</c> binding avoids
    ///     that risk entirely.</description></item>
    /// </list>
    /// At least one of the public dispatcher targets (<c>GetKerbinDateFromUT</c>) is positively confirmed by the
    /// stack traces we have observed, so the protection holds even if the other two targets are missing on a
    /// given build. The clamp threshold is held well below the smaller of the two formatters' overflow points and
    /// far above any realistic gameplay UT (~32 million Earth years), so well-behaved sessions are completely
    /// unaffected.
    /// </para>
    /// </summary>
    public static class DefaultDateTimeFormatterClamp
    {
        /// <summary>
        /// Maximum absolute UT we will hand to <see cref="KSPUtil.DefaultDateTimeFormatter"/>. Chosen as an
        /// order-of-magnitude guard well below the smaller (Kerbin) overflow threshold of ~1.97e16 s, and far
        /// above any realistic in-game UT (~32 million Earth years).
        /// </summary>
        public const double SafeMaxTimeSeconds = 1e15;

        public static double Clamp(double time)
        {
            if (double.IsNaN(time) || double.IsInfinity(time)) return 0.0;
            if (time > SafeMaxTimeSeconds) return SafeMaxTimeSeconds;
            if (time < -SafeMaxTimeSeconds) return -SafeMaxTimeSeconds;
            return time;
        }

        /// <summary>
        /// Install all three guard prefixes. Called once from <c>HarmonyPatcher.Awake</c> after <c>PatchAll</c>.
        /// Silently skips any target that is unreachable on the active KSP build.
        /// </summary>
        public static void Install(HarmonyLib.Harmony harmony)
        {
            // Public(ish) dispatchers - either is sufficient on its own to cover both planetarium UI and
            // StatusTexts. Patch both so RSS / planet-pack home worlds (Earth calendar) are also covered.
            TryPatchSingleDoubleParam(harmony, "GetKerbinDateFromUT");
            TryPatchSingleDoubleParam(harmony, "GetEarthDateFromUT");

            // Defense in depth: the private helper that actually performs the unsafe int cast. If both
            // dispatcher patches above succeed this is redundant; if neither dispatcher exists by these
            // exact names on this KSP build, this is the last line of defence before the throw.
            TryPatchHelper(harmony);
        }

        private static void TryPatchSingleDoubleParam(HarmonyLib.Harmony harmony, string methodName)
        {
            try
            {
                var target = AccessTools.Method(
                    typeof(KSPUtil.DefaultDateTimeFormatter),
                    methodName,
                    new[] { typeof(double) });

                if (target == null) return;

                var prefix = new HarmonyMethod(typeof(DefaultDateTimeFormatterClamp)
                    .GetMethod(nameof(SingleDoubleParamPrefix), BindingFlags.NonPublic | BindingFlags.Static));

                harmony.Patch(target, prefix: prefix);
            }
            catch (Exception)
            {
                // Method shape changed or unreachable on this KSP build - skip silently.
            }
        }

        private static void TryPatchHelper(HarmonyLib.Harmony harmony)
        {
            try
            {
                var target = AccessTools.Method(
                    typeof(KSPUtil.DefaultDateTimeFormatter),
                    "get_date_from_UT",
                    new[] { typeof(double), typeof(int), typeof(int) });

                if (target == null) return;

                var prefix = new HarmonyMethod(typeof(DefaultDateTimeFormatterClamp)
                    .GetMethod(nameof(HelperPrefix), BindingFlags.NonPublic | BindingFlags.Static));

                harmony.Patch(target, prefix: prefix);
            }
            catch (Exception)
            {
                // Helper signature changed or unreachable on this KSP build - skip silently.
            }
        }

        // Bound positionally via Harmony's __0 convention so we do not depend on the shipped assembly
        // preserving the original parameter name "time".
        private static void SingleDoubleParamPrefix(ref double __0)
        {
            __0 = Clamp(__0);
        }

        private static void HelperPrefix(ref double __0)
        {
            __0 = Clamp(__0);
        }
    }
}
