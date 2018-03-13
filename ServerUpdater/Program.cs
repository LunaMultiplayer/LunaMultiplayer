using CommonUpdater;
using System;
using System.IO;
using System.Threading;

namespace ServerUpdater
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("This program will download the latest UNSTABLE version and replace your current LMPServer");

            if (!File.Exists($@"{Directory.GetCurrentDirectory()}\Server.exe"))
                Console.WriteLine("Please drop \"Server LMP Updater\" in the LMP server folder next to Server.exe!");
            else
            {
                Downloader.DownloadAndReplaceFiles(ProductToDownload.Server);
            }

            Thread.Sleep(3000);
        }
    }
}
