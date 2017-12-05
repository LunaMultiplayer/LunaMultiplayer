using MasterServer;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Updater;

namespace LunaServer
{
    /// <summary>
    /// This program is the one who does the punchtrough between a nat client and a nat server. 
    /// You should only run if you agree in the forum to do so and your server ip is listed in:
    /// https://raw.githubusercontent.com/DaggerES/LunaMultiPlayer/master/MasterServersList
    /// </summary>
    /// 
    internal class Program
    {
        private static string ServerPipeHandle { get; set; }
        private static void Main()
        {
            if (!File.Exists(Constants.DllPath))
            {
                ConsoleLogger.Log(LogLevels.Error, $"Cannot find needed file {Constants.DllFileName}");
                return;
            }

            Constants.CurrentVersion = new Version(FileVersionInfo.GetVersionInfo(Constants.DllPath).FileVersion);

            Task.Run(() => ReadInputsAndSendToServer());
            Start();
            CheckNewVersion();

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        private static void ReadInputsAndSendToServer()
        {
            using (var serverPipe = new AnonymousPipeServerStream(PipeDirection.Out))
            using (var writer = new StreamWriter(serverPipe))
            {
                ServerPipeHandle = serverPipe.GetClientHandleAsString();
                writer.AutoFlush = true;
                while (true)
                {
                    var input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input))
                    {
                        serverPipe.WaitForPipeDrain();
                        writer.WriteLine(input);
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private static void Start()
        {
            Constants.LmpDomain = AppDomain.CreateDomain("LMP.Server", null, Constants.DomainSetup);
            Constants.LmpDomain.SetData("Stop", false);
            Constants.LmpDomain.SetData("LMPServerPipe", ServerPipeHandle);

            Task.Run(() =>
            Constants.LmpDomain.DoCallBack(() =>
            {
                var assemblyBytes = File.ReadAllBytes(Constants.DllPath);
                var assembly = AppDomain.CurrentDomain.Load(assemblyBytes);
                var entryPoint = assembly.GetType("LMP.Server.EntryPoint");

                entryPoint.GetMethod("MainEntryPoint")?.Invoke(null, new object[0]);
                while (!(bool)AppDomain.CurrentDomain.GetData("Stop"))
                {
                    Task.Delay(50);
                }

                var stopMethod = entryPoint.GetMethod("Stop");
                stopMethod?.Invoke(null, new object[0]);
            }));
        }

        private static void Stop()
        {
            Constants.LmpDomain.SetData("Stop", true);
            Thread.Sleep(30000);
            AppDomain.Unload(Constants.LmpDomain);
        }

        private static void CheckNewVersion()
        {
            Task.Run(() =>
            {
                while (true)
                {   
                    //Sleep for 3 minutes...
                    Task.Delay(1000);

                    var latestVersion = UpdateChecker.GetLatestVersion();
                    if (latestVersion > Constants.CurrentVersion)
                    {
                        ConsoleLogger.Log(LogLevels.Normal,
                            "Found a new updated version!. Downloading and restarting program....");

                        var url = UpdateDownloader.GetZipFileUrl(Constants.DebugVersion);
                        if (!string.IsNullOrEmpty(url))
                        {
                            var zipFileName = url.Substring(url.LastIndexOf("/") + 1);
                            UpdateDownloader.DownloadZipFile(url, Directory.GetCurrentDirectory() + "\\" + zipFileName);

                            Stop();

                            UpdateExtractor.ExtractZipFileToDirectory(Directory.GetCurrentDirectory() + "\\" + zipFileName, Directory.GetCurrentDirectory(),
                                UpdateExtractor.ProductToExtract.Server);

                            Start();
                        }
                    }
                }
            });
        }
    }
}
