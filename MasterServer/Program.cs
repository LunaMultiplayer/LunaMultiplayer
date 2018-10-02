using LmpUpdater;
using LmpUpdater.Appveyor;
using LmpUpdater.Github;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer
{
    /// <summary>
    /// This program is the one who does the punchtrough between a nat client and a nat server. 
    /// You should only run if you agree in the forum to do so and your server ip is listed in:
    /// https://raw.githubusercontent.com/LunaMultiplayer/LunaMultiplayer/master/MasterServersList
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
        private const string DllFileName = "LmpMasterServer.dll";

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);
        private static readonly string DllPath = Path.Combine(Directory.GetCurrentDirectory(), DllFileName);
        private static readonly AppDomainSetup DomainSetup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };

        private static Version CurrentVersion
        {
            get
            {
                var dllVersion = FileVersionInfo.GetVersionInfo(DllPath).FileVersion;
                var versionComponents = dllVersion.Split('.');

                return new Version(int.Parse(versionComponents[0]), int.Parse(versionComponents[1]), int.Parse(versionComponents[2]));
            }
        }

        private static AppDomain LmpDomain { get; set; }
        private static string[] Arguments { get; set; }

        #endregion

        private static void Main(string[] args)
        {
            //Uncomment this to properly debug the code
            //EntryPoint.MainEntryPoint(new string[0]);
            //while (true) { Thread.Sleep(100); }

            if (!File.Exists(DllPath))
            {
                ConsoleLogger.Log(LogLevels.Error, $"Cannot find needed file {DllFileName}");
                return;
            }

            Arguments = args;

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                StopMasterServerDll();
                QuitEvent.Set();
                eArgs.Cancel = true;
            };

            CheckNewVersion(args.Any(a => a.Contains("nightly")));
            StartMasterServerDll();
            QuitEvent.WaitOne();
        }

        /// <summary>
        /// Starts the master server dll
        /// </summary>
        private static void StartMasterServerDll()
        {
            LmpDomain = AppDomain.CreateDomain("LmpMasterServer", null, DomainSetup);
            LmpDomain.SetData("Arguments", Arguments);
            LmpDomain.SetData("Stop", false);

            LmpDomain.DoCallBack(async () =>
            {
                //Reload the uhttpSharp dll as otherwise on linux it fails
                var uhttpSharpPath = Path.Combine(Directory.GetCurrentDirectory(), "uhttpsharp.dll");
                AppDomain.CurrentDomain.Load(File.ReadAllBytes(uhttpSharpPath));

                var assembly = AppDomain.CurrentDomain.Load(File.ReadAllBytes(DllPath));
                var entryPoint = assembly.GetType("LmpMasterServer.EntryPoint");

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
        private static void StopMasterServerDll()
        {
            LmpDomain.SetData("Stop", true);
            Thread.Sleep(5000);
            AppDomain.Unload(LmpDomain);
            Console.Clear();
        }

        /// <summary>
        /// Checks if a new version exists and if it does, it stops the master server, downloads it and restarts it again
        /// </summary>
        private static void CheckNewVersion(bool nightly)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var latestVersion = nightly ? AppveyorUpdateChecker.GetLatestVersion() : GithubUpdateChecker.GetLatestVersion();
                    if (latestVersion > CurrentVersion)
                    {
                        var url = AppveyorUpdateDownloader.GetZipFileUrl(AppveyorProduct.MasterServer, DebugVersion);
                        if (!string.IsNullOrEmpty(url))
                        {
                            ConsoleLogger.Log(LogLevels.Normal, $"Found a new updated version! Current: {CurrentVersion} Latest: {latestVersion}");
                            ConsoleLogger.Log(LogLevels.Normal, "Downloading and restarting program....");

                            var zipFileName = url.Substring(url.LastIndexOf("/") + 1);
                            if (CommonDownloader.DownloadZipFile(url, Path.Combine(Directory.GetCurrentDirectory(), zipFileName)))
                            {
                                StopMasterServerDll();

                                AppveyorUpdateExtractor.ExtractZipFileToDirectory(Path.Combine(Directory.GetCurrentDirectory(), zipFileName), Directory.GetCurrentDirectory(),
                                    AppveyorProduct.MasterServer);

                                StartMasterServerDll();
                            }
                        }
                    }

                    //Sleep for 5 minutes...
                    await Task.Delay(5 * 1000 * 60);
                }
            });
        }
    }
}
