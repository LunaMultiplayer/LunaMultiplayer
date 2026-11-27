using LmpClient.Utilities;
using LmpCommon.CraftLibrary;
using System;
using System.IO;

namespace LmpClient.Systems.CraftLibrary
{
    /// <summary>
    /// Renames .craft files with no PART blocks in LMP's craft folders before KSP's editor can load them.
    /// Files are renamed to .craft.invalid, never deleted, so a bad copy can be recovered by hand.
    /// Runs at startup, which catches files restored by Steam Cloud and copies made outside LMP.
    /// </summary>
    public static class CraftQuarantine
    {
        /// <summary>
        /// Scans the VAB, SPH, and Subassemblies folders under the LMP save folder.
        /// </summary>
        public static void QuarantinePartlessCrafts(string lunaMultiplayerSaveFolder)
        {
            var folders = new[]
            {
                CommonUtil.CombinePaths(lunaMultiplayerSaveFolder, CommonUtil.CombinePaths("Ships", "VAB")),
                CommonUtil.CombinePaths(lunaMultiplayerSaveFolder, CommonUtil.CombinePaths("Ships", "SPH")),
                CommonUtil.CombinePaths(lunaMultiplayerSaveFolder, "Subassemblies")
            };

            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder)) continue;

                foreach (var file in Directory.GetFiles(folder, "*.craft"))
                    QuarantineIfPartless(file);
            }
        }

        private static void QuarantineIfPartless(string file)
        {
            try
            {
                var data = File.ReadAllBytes(file);
                if (CraftFileValidator.HasParts(data, data.Length)) return;

                var target = NextFreeQuarantineName(file);
                File.Move(file, target);
                LunaLog.LogWarning($"[LMP]: Moved craft with no parts out of the editor's way: {file} -> {target}");
            }
            catch (Exception ex)
            {
                LunaLog.LogError($"[LMP]: Could not check craft {file}: {ex.Message}");
            }
        }

        private static string NextFreeQuarantineName(string file)
        {
            var target = $"{file}.invalid";
            for (var i = 2; File.Exists(target); i++)
                target = $"{file}.{i}.invalid";

            return target;
        }
    }
}
