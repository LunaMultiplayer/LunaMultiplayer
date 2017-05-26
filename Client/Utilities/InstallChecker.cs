using System.IO;
using System.Reflection;
using UnityEngine;

namespace LunaClient.Utilities
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    internal class InstallChecker : MonoBehaviour
    {
        private static string _currentPath = "";
        private static string _correctPath = "";

        public static bool IsCorrectlyInstalled()
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly.Location == null)
                return false;

            var assemblyInstalledAt = new DirectoryInfo(assembly.Location).FullName;
            var kspPath = new DirectoryInfo(Client.KspPath).FullName;
            var shouldBeInstalledAt = CommonUtil.CombinePaths(kspPath, "GameData", "LunaMultiPlayer", "Plugins", "LunaClient.dll");

            _currentPath = assemblyInstalledAt;
            _correctPath = shouldBeInstalledAt;

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
                Debug.Log($"[InstallChecker] LMP is Currently installed on '{_currentPath}', should be installed at '{_correctPath}'");
                PopupDialog.SpawnPopupDialog(new Vector2(0, 0),
                    new Vector2(float.PositiveInfinity, float.PositiveInfinity), "Incorrect Install Detected",
                    $"LunaMultiPlayer is not correctly installed.\n\nCurrent location: {_currentPath}\n\nCorrect location: {_correctPath}\n", "OK", false, HighLogic.UISkin);
            }
        }
    }
}