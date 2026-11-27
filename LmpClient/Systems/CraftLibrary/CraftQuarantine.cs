using LmpClient.Utilities;
using LmpCommon.CraftLibrary;
using System;
using System.IO;

namespace LmpClient.Systems.CraftLibrary
{
    /// <summary>
    /// Renames .craft files with no PART blocks in LMP's craft folders before KSP's editor can load them.
    /// Each is renamed to .craft.invalid, replacing any earlier quarantined copy so repeats don't pile up.
    /// Runs at startup, which catches files restored by Steam Cloud and copies made outside LMP.
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
            string[] files;
            try
            {
                if (!Directory.Exists(folder)) return;

                files = Directory.GetFiles(folder, "*.craft");
            }
            catch (Exception ex)
            {
                LunaLog.LogError($"[LMP]: Could not list crafts in {folder}: {ex.Message}");
                return;
            }

            foreach (var file in files)
                QuarantineIfPartless(file);
        }

        private static void QuarantineIfPartless(string file)
        {
            try
            {
                bool hasParts;
                using (var stream = File.OpenRead(file))
                    hasParts = CraftFileValidator.HasParts(stream);

                if (hasParts) return;

                var target = $"{file}.invalid";
                if (File.Exists(target))
                    File.Delete(target);

                File.Move(file, target);
                LunaLog.LogWarning($"[LMP]: Moved craft with no parts out of the editor's way: {file} -> {target}");
            }
            catch (Exception ex)
            {
                LunaLog.LogError($"[LMP]: Could not check craft {file}: {ex.Message}");
            }
        }
    }
}
