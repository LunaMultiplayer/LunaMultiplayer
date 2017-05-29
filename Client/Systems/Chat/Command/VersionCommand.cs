using LunaCommon;

namespace LunaClient.Systems.Chat.Command
{
    public class VersionCommand
    {
        public void DisplayVersion(string commandArgs)
        {
            SystemsContainer.Get<ChatSystem>().PrintToSelectedChannel($"LunaMultiPlayer {VersionInfo.FullVersionNumber}");
        }
    }
}