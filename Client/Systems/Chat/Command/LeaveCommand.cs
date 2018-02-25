namespace LunaClient.Systems.Chat.Command
{
    public class LeaveCommand
    {
        public void LeaveChannel(string commandArgs)
        {
            ChatSystem.Singleton.LeaveEventHandled = false;
        }
    }
}