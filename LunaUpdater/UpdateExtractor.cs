using Ionic.Zip;
using System;
using System.IO;

namespace LunaUpdater
{
    public class UpdateExtractor
    {
        public enum ProductToExtract
        {
            Client,
            MasterServer,
            Server
        }

        public static void ExtractZipFileToDirectory(string zipFilePath, string destinationFolder, ProductToExtract product)
        {
            if (string.IsNullOrEmpty(zipFilePath) || string.IsNullOrEmpty(destinationFolder) || !Directory.Exists(destinationFolder) || !File.Exists(zipFilePath))
                return;

            using (var zipFile = ZipFile.Read(zipFilePath))
            {
                var tempFolder = destinationFolder + "\\TempUnzipFolder";
                Directory.CreateDirectory(tempFolder);
                zipFile.ExtractAll(tempFolder, ExtractExistingFileAction.OverwriteSilently);

                switch (product)
                {
                    case ProductToExtract.Client:
                        ExtractClient(tempFolder);
                        break;
                    case ProductToExtract.MasterServer:
                        ExtractMasterServer(tempFolder);
                        break;
                    case ProductToExtract.Server:
                        ExtractServer(tempFolder);
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
            var destFolder = tempFolder + "\\..";
            tempFolder += "\\LMPMasterServer";
            foreach (var file in Directory.GetFiles(tempFolder))
            {
                if (!Path.GetExtension(file).ToLower().Contains("exe"))
                {
                    try
                    {
                        File.Copy(file, destFolder + "//" + Path.GetFileName(file), true);
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
            throw new NotImplementedException();
        }

        private static void ExtractClient(string tempFolder)
        {
            throw new NotImplementedException();
        }
    }
}
