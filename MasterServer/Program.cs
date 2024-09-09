using LmpUpdater;
using LmpUpdater.Appveyor;
using LmpUpdater.Github;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer
{
    /// <summary>
    /// This program is the one who does the punchtrough between a nat client and a nat server.
    /// You should only run if you agree in the forum to do so and your server ip is listed in:
    /// https://raw.githubusercontent.com/LunaMultiplayer/LunaMultiplayer/master/MasterServersList
    /// </summary>
    internal class Program
    {
        #region Fields & properties

#if DEBUG
        private const bool DebugVersion = true;
#else
        private const bool DebugVersion = false;
#endif
        private const string DllFileName = "LmpMasterServer.dll";

        private static readonly string DllPath = Path.Combine(Directory.GetCurrentDirectory(), DllFileName);

        private static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);
        private static Action _stopDLLCallback;

        private static Version CurrentVersion
        {
            get
            {
                var dllVersion = FileVersionInfo.GetVersionInfo(DllPath).FileVersion;
                var versionComponents = dllVersion.Split('.');

                return new Version(
                    int.Parse(versionComponents[0]),
                    int.Parse(versionComponents[1]),
                    int.Parse(versionComponents[2])
                    );
            }
        }

        private static AppDomain LmpDomain { get; set; }
        private static string[] Arguments { get; set; }

        #endregion

        private static void Main(string[] args)
        {
            if (!File.Exists(DllPath))
            {
                LogError($"Cannot find needed file {DllFileName}");
                return;
            }

            Arguments = args;

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                StopMasterServerDll();
                QuitEvent.Set();
                eArgs.Cancel = true;
            };

            if (!args.Any(a => a.Contains("noupdatecheck")))
                CheckNewVersion(args.Any(a => a.Contains("nightly")));
            else
                Console.WriteLine("Automatic update checking disabled, please monitor for updates on your own.");
            StartMasterServerDll();
            QuitEvent.WaitOne();
        }

        /// <summary>
        /// Starts the master server dll
        /// </summary>
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, "LmpMasterServer.EntryPoint", "LmpMasterServer")]
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Required types specified above")]
#pragma warning disable IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
        private static void StartMasterServerDll()
        {
            // https://docs.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable#application-domains
            // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.loader.assemblyloadcontext?view=net-6.0
            var assmeblyLoadContext = new AssemblyLoadContext("LmpMasterServer", true);
            var assembly = assmeblyLoadContext.LoadFromAssemblyPath(DllPath);

            var entryPoint = assembly.GetType("LmpMasterServer.EntryPoint") ?? throw new Exception("Could not find entrypoint in LmpMasterServer DLL.");
            entryPoint.GetMethod("MainEntryPoint")?.Invoke(null, new object[] { Arguments });

            _stopDLLCallback = () =>
            {
                entryPoint.GetMethod("Stop")?.Invoke(null, new object[0]);
                assmeblyLoadContext.Unload();
            };
        }
//#pragma warning restore IL2075 // 'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.

        /// <summary>
        /// Stops the master server dll concurrent task
        /// </summary>
        private static void StopMasterServerDll()
        {
            _stopDLLCallback?.Invoke();
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
                            Console.WriteLine($"Found a new updated version! Current: {CurrentVersion} Latest: {latestVersion}");
                            Console.WriteLine("Downloading and restarting program....");

                            var zipFileName = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
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

        public static void LogError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ResetColor();
        }
    }
}
