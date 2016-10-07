using LunaClient.Base;
using LunaClient.Systems.Network;

namespace LunaClient.Systems.Chat.Command
{
    public class MotdCommand : SystemBase
    {
        public void ServerMotd(string commandArgs)
        {
            NetworkSystem.Singleton.SimpleMessageSender.SendMotdRequest();
        }
    }
}