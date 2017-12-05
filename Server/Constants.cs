using System;
using System.IO;

namespace LunaServer
{
    public class Constants
    {
        public static string DllFileName { get; } = "LMP.Server.dll";
        public static string DllPath { get; } = Directory.GetCurrentDirectory() + "\\" + DllFileName;
        public static Version CurrentVersion { get; set; }
        public static AppDomain LmpDomain { get; set; }
        public static AppDomainSetup DomainSetup { get; } = new AppDomainSetup() { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };

#if DEBUG
        public static bool DebugVersion { get; } = true;
#else
        public static bool DebugVersion { get; } = false;
#endif

    }
}
