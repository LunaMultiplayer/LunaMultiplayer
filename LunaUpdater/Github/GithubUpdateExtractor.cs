using Ionic.Zip;
using System;
using System.IO;

namespace LunaUpdater.Github
{
    public class GithubUpdateExtractor
    {
        public static void ExtractZipFileToDirectory(string zipFilePath, string destinationFolder, GithubProduct product)
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
                    case GithubProduct.Client:
                        ExtractClient(tempFolder);
                        break;
                    case GithubProduct.Server:
                        ExtractServer(tempFolder);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(product), product, null);
                }

                Directory.Delete(tempFolder, true);
            }

            File.Delete(zipFilePath);
        }

        private static void ExtractServer(string tempFolder)
        {
            var destFolder = Path.Combine(Path.Combine(tempFolder, ".."), "LMPServer");
            foreach (var file in Directory.GetFiles(tempFolder))
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
            var destFolder = Path.Combine(Path.Combine(tempFolder, ".."), "LMPClient");
            foreach (var file in Directory.GetFiles(tempFolder))
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
