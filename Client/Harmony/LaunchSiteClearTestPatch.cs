using Harmony;
using LunaCommon.Enums;
using PreFlightTests;
using System.Linq;

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fix the tests that are run before launching a vessel.
    /// When launching a vessel from editor or when clicking the runway KSC checks if there are vessels there.
    /// For LMP we just ignore that check by setting the "compatible" to false before doing the test and then 
    /// restoring it to true after the test is performed
    /// </summary>
    [HarmonyPatch(typeof(LaunchSiteClear))]
    [HarmonyPatch("Test")]
    public class LaunchSiteClearTestPatch
    {
        [HarmonyPrefix]
        private static void PreFixTest()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            var editorLogicInstances = UnityEngine.Object.FindObjectsOfType<EditorLogic>();
            if (editorLogicInstances != null && editorLogicInstances.Any())
            {
                foreach (var editorLogicInstance in editorLogicInstances)
                {
                    var launchSiteClear = Traverse.Create(editorLogicInstance).Field("launchSiteClearTest").GetValue<LaunchSiteClear>();
                    if (launchSiteClear != null)
                    {
                        var launchSiteClearGame = Traverse.Create(launchSiteClear).Field("st").GetValue<Game>();
                        launchSiteClearGame.compatible = false;
                    }
                }
            }

            var launchFacilitiesInstances = UnityEngine.Object.FindObjectsOfType<LaunchSiteFacility>();
            if (launchFacilitiesInstances != null && launchFacilitiesInstances.Any())
            {
                foreach (var launchFacilitiesInstance in launchFacilitiesInstances)
                {
                    var launchSiteClear = Traverse.Create(launchFacilitiesInstance).Field("launchSiteClearTest").GetValue<LaunchSiteClear>();
                    if (launchSiteClear != null)
                    {
                        var launchSiteClearGame = Traverse.Create(launchSiteClear).Field("st").GetValue<Game>();
                        launchSiteClearGame.compatible = false;
                    }
                }
            }
        }

        [HarmonyPostfix]
        private static void PostFixTest()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            var editorLogicInstances = UnityEngine.Object.FindObjectsOfType<EditorLogic>();
            if (editorLogicInstances != null && editorLogicInstances.Any())
            {
                foreach (var editorLogicInstance in editorLogicInstances)
                {
                    var launchSiteClear = Traverse.Create(editorLogicInstance).Field("launchSiteClearTest").GetValue<LaunchSiteClear>();
                    if (launchSiteClear != null)
                    {
                        var launchSiteClearGame = Traverse.Create(launchSiteClear).Field("st").GetValue<Game>();
                        launchSiteClearGame.compatible = true;
                    }
                }
            }

            var launchFacilitiesInstances = UnityEngine.Object.FindObjectsOfType<LaunchSiteFacility>();
            if (launchFacilitiesInstances != null && launchFacilitiesInstances.Any())
            {
                foreach (var launchFacilitiesInstance in launchFacilitiesInstances)
                {
                    var launchSiteClear = Traverse.Create(launchFacilitiesInstance).Field("launchSiteClearTest").GetValue<LaunchSiteClear>();
                    if (launchSiteClear != null)
                    {
                        var launchSiteClearGame = Traverse.Create(launchSiteClear).Field("st").GetValue<Game>();
                        launchSiteClearGame.compatible = true;
                    }
                }
            }
        }
    }
}
