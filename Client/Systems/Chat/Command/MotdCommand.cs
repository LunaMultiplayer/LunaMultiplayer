using LunaClient.Base;
using LunaClient.Network;

namespace LunaClient.Systems.Chat.Command
{
    public class MotdCommand : SystemBase
    {
        public void ServerMotd(string commandArgs)
        {
            NetworkSimpleMessageSender.SendMotdRequest();
        }
    }
}