using LmpClient.Localization;
using System.IO;
using System.Reflection;

namespace LmpClient.Utilities
{
    public class InstallChecker
    {
        private static string _currentPath = "";
        private static string _correctPath = "";

        public static bool IsCorrectlyInstalled()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var assemblyInstalledAt = new DirectoryInfo(assembly.Location).FullName;
            var kspPath = new DirectoryInfo(MainSystem.KspPath).FullName;
            var shouldBeInstalledAt = CommonUtil.CombinePaths(kspPath, "GameData", "LunaMultiplayer", "Plugins", "LmpClient.dll");

            _currentPath = assemblyInstalledAt;
            _correctPath = shouldBeInstalledAt;

            if (File.Exists(shouldBeInstalledAt))
                return true;

            return assemblyInstalledAt == shouldBeInstalledAt;
        }

        public static void SpawnDialog()
        {
            if (IsCorrectlyInstalled()) return;

            LunaLog.Log($"[InstallChecker] Mod '{Assembly.GetExecutingAssembly().GetName().Name}' is not correctly installed.");
            LunaLog.Log($"[InstallChecker] LMP is Currently installed on '{_currentPath}', should be installed at '{_correctPath}'");
            PopupDialog.SpawnPopupDialog(new MultiOptionDialog("InstallChecker", LocalizationContainer.InstallDialogText.IncorrectInstall + "\n\n" +
                                                                                 LocalizationContainer.InstallDialogText.CurrentLoc + " " + _currentPath + "\n\n" +
                                                                                 LocalizationContainer.InstallDialogText.CorrectLoc + " " + _correctPath + "\n",
                LocalizationContainer.InstallDialogText.Title, HighLogic.UISkin), true, HighLogic.UISkin);
        }
    }
}