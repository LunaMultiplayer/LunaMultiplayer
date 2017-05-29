namespace LunaClient.Systems.Chat.Command
{
    public class LeaveCommand
    {
        public void LeaveChannel(string commandArgs)
        {
            SystemsContainer.Get<ChatSystem>().LeaveEventHandled = false;
        }
    }
}