using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibraryMessageSender : SubSystem<CraftLibrarySystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSystem.Singleton.QueueOutgoingMessage(MessageFactory.CreateNew<CraftLibraryCliMsg>(msg));
        }
    }
}