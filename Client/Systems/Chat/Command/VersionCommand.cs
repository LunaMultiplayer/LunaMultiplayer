using LunaCommon;

namespace LunaClient.Systems.Chat.Command
{
    public class VersionCommand
    {
        public void DisplayVersion(string commandArgs)
        {
            ChatSystem.Singleton.PrintToSelectedChannel($"LunaMultiplayer {LmpVersioning.CurrentVersion}");
        }
    }
}