using System;
using System.IO;

namespace LmpCommon.CraftLibrary
{
    /// <summary>
    /// Renames .craft files with no PART blocks to .craft.invalid so KSP's editor cannot load them.
    /// An earlier quarantined copy is replaced in one step, so repeated restores don't pile up files.
    /// Never throws; problems are returned in the report so a bad folder cannot block startup.
    /// </summary>
    public static class CraftFolderQuarantine
    {
        private const string QuarantineExtension = ".invalid";

        /// <summary>
        /// Checks every .craft file in the folder and quarantines the ones with no parts.
        /// </summary>
        public static CraftQuarantineReport QuarantinePartlessCrafts(string folder, SearchOption searchOption)
        {
            var report = new CraftQuarantineReport();

            string[] files;
            try
            {
                if (!Directory.Exists(folder)) return report;

                files = Directory.GetFiles(folder, "*.craft", searchOption);
            }
            catch (Exception ex)
            {
                report.AddFailure(folder, $"Could not list crafts: {ex.Message}");
                return report;
            }

            foreach (var file in files)
                QuarantineIfPartless(file, report);

            return report;
        }

        private static void QuarantineIfPartless(string file, CraftQuarantineReport report)
        {
            bool hasParts;
            try
            {
                using (var stream = File.OpenRead(file))
                    hasParts = CraftFileValidator.HasParts(stream);
            }
            catch (Exception ex)
            {
                report.AddFailure(file, $"Could not check craft: {ex.Message}");
                return;
            }

            if (hasParts) return;

            var target = file + QuarantineExtension;
            try
            {
                MoveToQuarantine(file, target);
                report.AddMoved(file, target);
            }
            catch (Exception ex)
            {
                report.AddFailure(file, $"Could not quarantine craft: {ex.Message}");
            }
        }

        private static void MoveToQuarantine(string file, string target)
        {
            if (!File.Exists(target))
            {
                File.Move(file, target);
                return;
            }

            try
            {
                File.Replace(file, target, null);
            }
            catch (Exception)
            {
                // Not every runtime (KSP's Mono included) is guaranteed to support a one-step replace,
                // and an old quarantined copy is worth less than getting the partless file out of the editor's way
                File.Delete(target);
                File.Move(file, target);
            }
        }
    }
}
