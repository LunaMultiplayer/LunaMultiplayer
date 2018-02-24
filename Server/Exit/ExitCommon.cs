using LunaCommon;

namespace Server.Exit
{
    public static class ExitCommon
    {
        public static IExitSignal GetExitHandler()
        {
            if (Common.PlatformIsWindows())
                return new WinExitSignal();
            return new UnixExitSignal();
        }
    }
}
