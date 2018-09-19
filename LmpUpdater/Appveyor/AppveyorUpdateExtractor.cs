using Ionic.Zip;
using System;
using System.IO;

namespace LmpUpdater.Appveyor
{
    public class AppveyorUpdateExtractor
    {
        public static void ExtractZipFileToDirectory(string zipFilePath, string destinationFolder, AppveyorProduct product)
        {
            if (string.IsNullOrEmpty(zipFilePath) || string.IsNullOrEmpty(destinationFolder) || !Directory.Exists(destinationFolder) || !File.Exists(zipFilePath))
                return;

            using (var zipFile = ZipFile.Read(zipFilePath))
            {
                var tempFolder = Path.Combine(destinationFolder, "TempUnzipFolder");
                Directory.CreateDirectory(tempFolder);
                zipFile.ExtractAll(tempFolder, ExtractExistingFileAction.OverwriteSilently);

                switch (product)
                {
                    case AppveyorProduct.Client:
                        ExtractClient(tempFolder);
                        break;
                    case AppveyorProduct.Server:
                        ExtractServer(tempFolder);
                        break;
                    case AppveyorProduct.MasterServer:
                        ExtractMasterServer(tempFolder);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(product), product, null);
                }

                Directory.Delete(tempFolder, true);
            }

            File.Delete(zipFilePath);
        }

        private static void ExtractMasterServer(string tempFolder)
        {
            var destFolder = Path.Combine(tempFolder, "..");
            foreach (var file in Directory.GetFiles(Path.Combine(tempFolder, "LMPMasterServer")))
            {
                if (!Path.GetExtension(file).ToLower().Contains("exe"))
                {
                    try
                    {
                        File.Copy(file, Path.Combine(destFolder, Path.GetFileName(file)), true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private static void ExtractServer(string tempFolder)
        {
            var destFolder = Path.Combine(tempFolder, "..");
            foreach (var file in Directory.GetFiles(Path.Combine(tempFolder, "LMPServer")))
            {
                if (!Path.GetExtension(file).ToLower().Contains("exe"))
                {
                    try
                    {
                        File.Copy(file, Path.Combine(destFolder, Path.GetFileName(file)), true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        private static void ExtractClient(string tempFolder)
        {
            var destFolder = Path.Combine(tempFolder, "..");
            foreach (var file in Directory.GetFiles(Path.Combine(tempFolder, "LMPClient")))
            {
                if (!Path.GetExtension(file).ToLower().Contains("exe"))
                {
                    try
                    {
                        File.Copy(file, Path.Combine(destFolder, Path.GetFileName(file)), true);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
