using System.Reflection;

namespace LunaCommon
{
    public static class VersionInfo
    {
        public static string FullVersionNumber { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}