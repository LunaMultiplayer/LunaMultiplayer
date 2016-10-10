using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Systems.Network;

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