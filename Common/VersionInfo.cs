using System.Reflection;

namespace LunaCommon
{
    public static class VersionInfo
    {
        public static string FullVersionNumber { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string VersionNumber { get; } = FullVersionNumber.Substring(0, FullVersionNumber.LastIndexOf('.'));
    }
}