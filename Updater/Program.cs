using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Updater.Structures;

namespace Updater
{
    public class Program
    {
        public const string ApiUrl = "https://ci.appveyor.com/api";
        public static string ProjectUrl = $"{ApiUrl}/projects/gavazquez/lunamultiplayer";

        public static void Main(string[] args)
        {
            Console.WriteLine("This program will download the latest UNSTABLE version and replace your current LMP");

            if (!File.Exists($@"{Directory.GetCurrentDirectory()}\KSP.exe"))
                Console.WriteLine("Please drop LMP Updater in the main KSP folder next to KSP.exe!");
            else
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
                            client.DownloadFile(downloadUrl, $"{Path.GetTempPath()}LunaMultiPlayer-Debug.zip");
                            Console.WriteLine($"Downloading succeeded! Path: {Path.GetTempPath()}LunaMultiPlayer-Debug.zip");
                        }

                        Console.WriteLine($"Decompressing file to {Path.GetTempPath()}LMP");
                        ZipFile.ExtractToDirectory($"{Path.GetTempPath()}LunaMultiPlayer-Debug.zip", $"{Path.GetTempPath()}LMP");

                        CopyFilesFromTempToDestination();

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

            Thread.Sleep(3000);
        }

        private static void CopyFilesFromTempToDestination()
        {
            foreach (var dirPath in Directory.GetDirectories($@"{Path.GetTempPath()}LMP\LMPClient", "*", SearchOption.AllDirectories))
            {
                var destFolder = dirPath.Replace($@"{Path.GetTempPath()}LMP\LMPClient", $@"{Directory.GetCurrentDirectory()}");
                Console.WriteLine($"Creatind dest folder: {destFolder}");
                Directory.CreateDirectory(destFolder);
            }

            foreach (var newPath in Directory.GetFiles($@"{Path.GetTempPath()}LMP\LMPClient", "*.*", SearchOption.AllDirectories))
            {
                var destPath = newPath.Replace($@"{Path.GetTempPath()}LMP\LMPClient", $@"{Directory.GetCurrentDirectory()}");
                Console.WriteLine($"Copying {Path.GetFileName(newPath)} to {destPath}");
                File.Copy(newPath, destPath, true);
            }
        }

        private static void CleanTempFiles()
        {
            if (Directory.Exists($"{Path.GetTempPath()}LMP"))
                Directory.Delete($"{Path.GetTempPath()}LMP", true);
            File.Delete($"{Path.GetTempPath()}LunaMultiPlayer-Debug.zip");
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
                        return $"{ApiUrl}/buildjobs/{job.jobId}/artifacts/LunaMultiPlayer-Debug.zip";
                    }
                }
                else
                {
                    Console.WriteLine("Latest build was not a success. Cannot download");
                }
            }

            return null;
        }
    }
}
