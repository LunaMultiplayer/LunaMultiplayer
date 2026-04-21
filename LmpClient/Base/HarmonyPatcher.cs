using System;
using System.Reflection;
namespace LmpClient.Base
{
    public static class HarmonyPatcher
    {
        public static HarmonyLib.Harmony HarmonyInstance = new HarmonyLib.Harmony("LunaMultiplayer");

        public static void Awake()
        {
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            PatchOptionalMods();
        }

        /// <summary>
        /// Runtime patches for mods that aren't compile-time dependencies.
        /// Each patch is wrapped in try/catch so missing mods are silently skipped.
        /// </summary>
        private static void PatchOptionalMods()
        {
            SuppressClickThroughBlockerPopup();
            PatchContractPreLoader();
        }

        /// <summary>
        /// Patches <c>ContractConfigurator.ContractPreLoader.OnLoad</c> with a prefix that
        /// strips CONTRACT nodes containing unknown or malformed parameters before CC's
        /// code iterates them.
        ///
        /// This must be done imperatively rather than via <c>[HarmonyPatch]</c> attributes
        /// because <c>ContractPreLoader.OnLoad</c> is a virtual override of
        /// <c>ScenarioModule.OnLoad</c>.  An attribute-based patch targeting the base class
        /// method is never dispatched through for ContractPreLoader instances — the vtable
        /// jumps directly to the derived-class body, bypassing our patch entirely.
        /// </summary>
        internal static void PatchContractPreLoader()
        {
            try
            {
                var ccplType = HarmonyLib.AccessTools.TypeByName("ContractConfigurator.ContractPreLoader");
                if (ccplType == null)
                {
                    LunaLog.Log("[LMP]: ContractConfigurator.ContractPreLoader type not found — CC not installed, skipping contract pre-filter patch.");
                    return;
                }

                var onLoad = HarmonyLib.AccessTools.Method(ccplType, "OnLoad");
                if (onLoad == null)
                {
                    LunaLog.LogWarning("[LMP]: ContractPreLoader.OnLoad method not found — CC version mismatch?");
                    return;
                }

                var prefix = new HarmonyLib.HarmonyMethod(typeof(LmpClient.Harmony.ContractPreLoader_Filter), "Prefix");
                HarmonyInstance.Patch(onLoad, prefix: prefix);
                LunaLog.Log("[LMP]: Patched ContractConfigurator.ContractPreLoader.OnLoad — invalid contracts will be filtered before CC loads them.");
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: Could not patch ContractPreLoader.OnLoad: {e.Message}");
            }
        }

        /// <summary>
        /// CTB's OneTimePopup shows every time you enter the KSC scene. It reads
        /// PopUpShown.cfg to check if it was already shown, but something in LMP's
        /// game creation process causes it to re-trigger on every server join.
        /// Since the user has already configured CTB (PopUpShown.cfg = true),
        /// suppress the popup entirely when LMP is loaded.
        /// </summary>
        private static void SuppressClickThroughBlockerPopup()
        {
            try
            {
                // CTB's OneTimePopup class is in the root namespace (no namespace),
                // not "ClickThroughBlocker.OneTimePopup" despite the assembly name.
                var popupType = HarmonyLib.AccessTools.TypeByName("OneTimePopup");
                if (popupType == null)
                {
                    LunaLog.Log("[LMP]: ClickThroughBlocker OneTimePopup type not found — mod not installed, skipping");
                    return;
                }

                var startMethod = HarmonyLib.AccessTools.Method(popupType, "Start");
                if (startMethod == null)
                {
                    LunaLog.LogWarning("[LMP]: OneTimePopup.Start method not found — CTB version mismatch?");
                    return;
                }

                var prefix = new HarmonyLib.HarmonyMethod(typeof(HarmonyPatcher), nameof(SkipMethod));
                HarmonyInstance.Patch(startMethod, prefix: prefix);
                LunaLog.Log("[LMP]: Patched OneTimePopup.Start — CTB popup suppressed");
            }
            catch (Exception e)
            {
                LunaLog.LogWarning($"[LMP]: Could not patch ClickThroughBlocker popup: {e.Message}");
            }
        }

        /// <summary>
        /// Generic prefix that skips the original method entirely.
        /// </summary>
        private static bool SkipMethod() => false;
    }
}