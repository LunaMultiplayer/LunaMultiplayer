using System.Net.Mime;
using System.Reflection;

namespace LunaCommon
{
    public static class VersionInfo
    {
        public static string VersionNumber { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}