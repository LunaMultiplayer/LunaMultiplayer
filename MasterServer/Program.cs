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
        #region Fields & properties

#if DEBUG
        private const bool DebugVersion = true;
#else
        private const bool DebugVersion = false;
#endif
        private const string DllFileName = "LMP.MasterServer.dll";

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);
        private static readonly string DllPath = Directory.GetCurrentDirectory() + "\\" + DllFileName;
        private static readonly AppDomainSetup DomainSetup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };

        private static Version CurrentVersion { get; set; }
        private static AppDomain LmpDomain { get; set; }
        private static string[] Arguments { get; set; }

        #endregion

        private static void Main(string[] args)
        {
            if (!File.Exists(DllPath))
            {
                ConsoleLogger.Log(LogLevels.Error, $"Cannot find needed file {DllFileName}");
                return;
            }

            Arguments = args;
            CurrentVersion = new Version(FileVersionInfo.GetVersionInfo(DllPath).FileVersion);

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                QuitEvent.Set();
                eArgs.Cancel = true;
            };

            CheckNewVersion();
            StartMasterServerDll();
            QuitEvent.WaitOne();
        }

        /// <summary>
        /// Starts the master server dll
        /// </summary>
        private static void StartMasterServerDll()
        {
            LmpDomain = AppDomain.CreateDomain("LMP.MasterServer", null, DomainSetup);
            LmpDomain.SetData("Arguments", Arguments);
            LmpDomain.SetData("Stop", false);

            LmpDomain.DoCallBack(async () =>
            {
                var assembly = AppDomain.CurrentDomain.Load(File.ReadAllBytes(DllPath));
                var entryPoint = assembly.GetType("LMP.MasterServer.EntryPoint");

                entryPoint.GetMethod("MainEntryPoint")?.Invoke(null, new[] { AppDomain.CurrentDomain.GetData("Arguments") });

                while (!(bool)AppDomain.CurrentDomain.GetData("Stop"))
                {
                    await Task.Delay(100);
                }

                entryPoint.GetMethod("Stop")?.Invoke(null, new object[0]);
            });
        }

        /// <summary>
        /// Stops the master server dll concurrent task
        /// </summary>
        private static async void StopMasterServerDll()
        {
            LmpDomain.SetData("Stop", true);
            await Task.Delay(5000);
            AppDomain.Unload(LmpDomain);
            Console.Clear();
        }

        /// <summary>
        /// Checks if a new version exists and if it does, it stops the master server, downloads it and restarts it again
        /// </summary>
        private static void CheckNewVersion()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var latestVersion = UpdateChecker.GetLatestVersion();
                    if (latestVersion > CurrentVersion)
                    {
                        ConsoleLogger.Log(LogLevels.Normal, "Found a new updated version!. Downloading and restarting program....");

                        var url = UpdateDownloader.GetZipFileUrl(DebugVersion);
                        if (!string.IsNullOrEmpty(url))
                        {
                            var zipFileName = url.Substring(url.LastIndexOf("/") + 1);
                            UpdateDownloader.DownloadZipFile(url, Directory.GetCurrentDirectory() + "\\" + zipFileName);

                            StopMasterServerDll();

                            UpdateExtractor.ExtractZipFileToDirectory(Directory.GetCurrentDirectory() + "\\" + zipFileName, Directory.GetCurrentDirectory(),
                                UpdateExtractor.ProductToExtract.MasterServer);

                            StartMasterServerDll();
                        }
                    }

                    //Sleep for 30 minutes...
                    await Task.Delay(30 * 60 * 1000);
                }
            });
        }
    }
}
