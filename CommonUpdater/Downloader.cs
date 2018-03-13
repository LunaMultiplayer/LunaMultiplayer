using CommonUpdater.Structures;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace CommonUpdater
{
    public class Downloader
    {
        public const string ApiUrl = "https://ci.appveyor.com/api";
        public static string ProjectUrl = $"{ApiUrl}/projects/gavazquez/lunamultiplayer";
        public const string FileName = "LunaMultiplayer-Debug.zip";
        public static string FolderToDecompress = Path.Combine(Path.GetTempPath(), "LMP");

        public static void DownloadAndReplaceFiles(ProductToDownload product)
        {
            string downloadUrl;
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                downloadUrl = GetDownloadUrl(client).Result;
            }

            if (!string.IsNullOrEmpty(downloadUrl))
            {
                Console.WriteLine($"Downloading LMP from: {downloadUrl} Please wait...");
                try
                {
                    CleanTempFiles();
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(downloadUrl, Path.Combine(Path.GetTempPath(), FileName));
                        Console.WriteLine($"Downloading succeeded! Path: {Path.Combine(Path.GetTempPath(), FileName)}");
                    }

                    Console.WriteLine($"Decompressing file to {FolderToDecompress}");
                    ZipFile.ExtractToDirectory(Path.Combine(Path.GetTempPath(), FileName), FolderToDecompress);

                    CopyFilesFromTempToDestination(product);

                    Console.WriteLine("-----------------===========FINISHED===========-----------------");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    CleanTempFiles();
                }
            }
        }

        private static void CopyFilesFromTempToDestination(ProductToDownload product)
        {
            var productFolderName = GetProductFolderName(product);
            foreach (var dirPath in Directory.GetDirectories(Path.Combine(FolderToDecompress, productFolderName), "*", SearchOption.AllDirectories))
            {
                var destFolder = dirPath.Replace(Path.Combine(FolderToDecompress, productFolderName), Directory.GetCurrentDirectory());
                Console.WriteLine($"Creatind dest folder: {destFolder}");
                Directory.CreateDirectory(destFolder);
            }

            foreach (var newPath in Directory.GetFiles(Path.Combine(FolderToDecompress, productFolderName), "*.*", SearchOption.AllDirectories))
            {
                var destPath = newPath.Replace(Path.Combine(FolderToDecompress, productFolderName), Directory.GetCurrentDirectory());
                Console.WriteLine($"Copying {Path.GetFileName(newPath)} to {destPath}");
                File.Copy(newPath, destPath, true);
            }
        }

        private static string GetProductFolderName(ProductToDownload product)
        {
            switch (product)
            {
                case ProductToDownload.Client:
                    return "LMPClient";
                case ProductToDownload.Server:
                    return "LMPServer";
                default:
                    throw new ArgumentOutOfRangeException(nameof(product), product, null);
            }
        }

        private static void CleanTempFiles()
        {
            try
            {
                if (Directory.Exists(FolderToDecompress))
                    Directory.Delete(FolderToDecompress, true);
            }
            catch (Exception)
            {
                // ignored
            }

            File.Delete(Path.Combine(Path.GetTempPath(), FileName));
        }

        private static async Task<string> GetDownloadUrl(HttpClient client)
        {
            using (var response = await client.GetAsync(ProjectUrl))
            {
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var obj = new JavaScriptSerializer().Deserialize<RootObject>(content);
                if (obj.build.status == "success")
                {
                    var job = obj.build.jobs.FirstOrDefault(j => j.name.Contains("Debug"));
                    if (job != null)
                    {
                        Console.WriteLine($"Downloading DEBUG version: {obj.build.version}");
                        return $"{ApiUrl}/buildjobs/{job.jobId}/artifacts/{FileName}";
                    }
                }
                else
                {
                    Console.WriteLine($"Latest build status ({obj.build.status}) is not \"success\". Cannot download at this moment");
                }
            }

            return null;
        }
    }
}
