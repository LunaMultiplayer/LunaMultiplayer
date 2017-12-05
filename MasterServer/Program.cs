using LunaUpdater;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer
{
    /// <summary>
    /// This program is the one who does the punchtrough between a nat client and a nat server. 
    /// You should only run if you agree in the forum to do so and your server ip is listed in:
    /// https://raw.githubusercontent.com/DaggerES/LunaMultiPlayer/master/MasterServersList
    /// </summary>
    /// 
    internal class Program
    {


        private static void Main(string[] args)
        {
            if (!File.Exists(Constants.DllPath))
            {
                ConsoleLogger.Log(LogLevels.Error, $"Cannot find needed file {Constants.DllFileName}");
                return;
            }

            Constants.CurrentVersion = new Version(FileVersionInfo.GetVersionInfo(Constants.DllPath).FileVersion);

            CheckNewVersion(args);
            Start(args);

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        private static void Start(string[] args)
        {
            Constants.LmpDomain = AppDomain.CreateDomain("LMP.MasterServer", null, Constants.DomainSetup);
            Constants.LmpDomain.SetData("Args", args);
            Constants.LmpDomain.SetData("Stop", false);

            Constants.LmpDomain.DoCallBack(() =>
            {
                var assemblyBytes = File.ReadAllBytes(Constants.DllPath);
                var assembly = AppDomain.CurrentDomain.Load(assemblyBytes);
                var entryPoint = assembly.GetType("LMP.MasterServer.EntryPoint");

                entryPoint.GetMethod("MainEntryPoint")?.Invoke(null, new[] { AppDomain.CurrentDomain.GetData("Args") });

                while (!(bool)AppDomain.CurrentDomain.GetData("Stop"))
                {
                    Thread.Sleep(50);
                }

                var stopMethod = entryPoint.GetMethod("Stop");
                stopMethod?.Invoke(null, new object[0]);
            });
        }
        
        private static void Stop()
        {
            Constants.LmpDomain.SetData("Stop", true);
            Thread.Sleep(5000);
            AppDomain.Unload(Constants.LmpDomain);
        }

        private static void CheckNewVersion(string[] args)
        {
            Task.Run(() =>
            {
                //Wait 5 seconds before checking...
                Task.Delay(5000);

                while (true)
                {
                    var latestVersion = UpdateChecker.GetLatestVersion();
                    if (latestVersion > Constants.CurrentVersion)
                    {
                        ConsoleLogger.Log(LogLevels.Normal, "Found a new updated version!. Downloading and restarting program....");

                        var url = UpdateDownloader.GetZipFileUrl(Constants.DebugVersion);
                        if (!string.IsNullOrEmpty(url))
                        {
                            var zipFileName = url.Substring(url.LastIndexOf("/") + 1);
                            UpdateDownloader.DownloadZipFile(url, Directory.GetCurrentDirectory() + "\\" + zipFileName);

                            Stop();

                            UpdateExtractor.ExtractZipFileToDirectory(Directory.GetCurrentDirectory() + "\\" + zipFileName, Directory.GetCurrentDirectory(),
                                UpdateExtractor.ProductToExtract.MasterServer);

                            Start(args);
                        }
                    }

                    //Sleep for 30 minutes...
                    Task.Delay(30 * 60 * 1000);
                }
            });
        }
    }
}
