using LunaClient.Localization;
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

            var assemblyInstalledAt = new DirectoryInfo(assembly.Location).FullName;
            var kspPath = new DirectoryInfo(MainSystem.KspPath).FullName;
            var shouldBeInstalledAt = CommonUtil.CombinePaths(kspPath, "GameData", "LunaMultiPlayer", "Plugins", "LunaClient.dll");

            _currentPath = assemblyInstalledAt;
            _correctPath = shouldBeInstalledAt;

            if (File.Exists(shouldBeInstalledAt))
                return true;

            return assemblyInstalledAt == shouldBeInstalledAt;
        }

        public void Awake()
        {
            LunaLog.Log($"[InstallChecker] Running checker from '{Assembly.GetExecutingAssembly().GetName().Name}'");

            if (!IsCorrectlyInstalled())
            {
                LunaLog.Log($"[InstallChecker] Mod '{Assembly.GetExecutingAssembly().GetName().Name}' is not correctly installed.");
                LunaLog.Log($"[InstallChecker] LMP is Currently installed on '{_currentPath}', should be installed at '{_correctPath}'");
                PopupDialog.SpawnPopupDialog(new MultiOptionDialog("InstallChecker", LocalizationContainer.InstallDialogText.IncorrectInstall +"\n\n" +
                                                                                     LocalizationContainer.InstallDialogText.CurrentLoc + " " + _currentPath + "\n\n" +
                                                                                     LocalizationContainer.InstallDialogText.CorrectLoc + " " + _correctPath + "\n",
                    LocalizationContainer.InstallDialogText.Title, HighLogic.UISkin), true, HighLogic.UISkin);
            }
        }
    }
}