using System.IO;
using System.Reflection;
using UnityEngine;

namespace LunaClient.Utilities
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    internal class InstallChecker : MonoBehaviour
    {
        private static string currentPath = "";
        private static string correctPath = "";

        public static bool IsCorrectlyInstalled()
        {
            var assemblyInstalledAt = new DirectoryInfo(Assembly.GetExecutingAssembly().Location).FullName;
            var kspPath = new DirectoryInfo(KSPUtil.ApplicationRootPath).FullName;
            var shouldBeInstalledAt = CommonUtil.CombinePaths(kspPath, "GameData", "LunaMultiPlayer", "Plugins", "LunaClient.dll");

            currentPath = assemblyInstalledAt;
            correctPath = shouldBeInstalledAt;

            if (File.Exists(shouldBeInstalledAt))
                return true;
            return assemblyInstalledAt == shouldBeInstalledAt;
        }

        public void Awake()
        {
            Debug.Log($"[InstallChecker] Running checker from '{Assembly.GetExecutingAssembly().GetName().Name}'");

            if (!IsCorrectlyInstalled())
            {
                Debug.Log($"[InstallChecker] Mod '{Assembly.GetExecutingAssembly().GetName().Name}' is not correctly installed.");
                Debug.Log($"[InstallChecker] LMP is Currently installed on '{currentPath}', should be installed at '{correctPath}'");
                PopupDialog.SpawnPopupDialog(new Vector2(0, 0),
                    new Vector2(float.PositiveInfinity, float.PositiveInfinity), "Incorrect Install Detected",
                    $"LunaMultiPlayer is not correctly installed.\n\nCurrent location: {currentPath}\n\nCorrect location: {correctPath}\n", "OK", false, HighLogic.UISkin);
            }
        }
    }
}