using LmpClient.Utilities;
using LmpCommon.CraftLibrary;
using System.IO;

namespace LmpClient.Systems.CraftLibrary
{
    /// <summary>
    /// Quarantines .craft files with no PART blocks in LMP's craft folders before KSP's editor can load them.
    /// Runs at startup, which catches files restored by Steam Cloud and copies made outside LMP.
    /// The move itself lives in LmpCommon; this class picks the folders and logs the result.
    /// </summary>
    public static class CraftQuarantine
    {
        /// <summary>
        /// Scans the VAB, SPH, and Subassemblies folders under the LMP save folder.
        /// </summary>
        public static void QuarantinePartlessCrafts(string lunaMultiplayerSaveFolder)
        {
            QuarantineFolder(CommonUtil.CombinePaths(lunaMultiplayerSaveFolder, CommonUtil.CombinePaths("Ships", "VAB")));
            QuarantineFolder(CommonUtil.CombinePaths(lunaMultiplayerSaveFolder, CommonUtil.CombinePaths("Ships", "SPH")));
            QuarantineFolder(CommonUtil.CombinePaths(lunaMultiplayerSaveFolder, "Subassemblies"));
        }

        private static void QuarantineFolder(string folder)
        {
            var report = CraftFolderQuarantine.QuarantinePartlessCrafts(folder, SearchOption.TopDirectoryOnly);

            foreach (var moved in report.Moved)
                LunaLog.LogWarning($"[LMP]: Moved craft with no parts out of the editor's way: {moved.Original} -> {moved.Quarantined}");

            foreach (var failure in report.Failures)
                LunaLog.LogError($"[LMP]: {failure.Message} ({failure.Path})");
        }
    }
}
