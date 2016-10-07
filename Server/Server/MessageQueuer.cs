using System.Linq;
using LunaCommon.Message.Data;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;

namespace LunaServer.Server
{
    public class MessageQueuer
    {
        public static void RelayMessage<T>(ClientStructure exceptClient, IMessageData data)
            where T : IServerMessageBase, new()
        {
            SendToAllOtherClients<T>(exceptClient, data);
        }

        private static void SendToAllOtherClients<T>(ClientStructure exceptClient, IMessageData data)
            where T : IServerMessageBase, new()
        {
            var newMessage = ServerContext.ServerMessageFactory.CreateNew<T>();
            newMessage.SetData(data);

            foreach (var otherClient in ServerContext.Clients.Values.Where(c => c != exceptClient))
                SendToClient(otherClient, newMessage);
        }

        public static void SendToAllClients<T>(IMessageData data)
            where T : IServerMessageBase, new()
        {
            var newMessage = ServerContext.ServerMessageFactory.CreateNew<T>();
            newMessage.SetData(data);

            foreach (var otherClient in ServerContext.Clients.Values)
                SendToClient(otherClient, newMessage);
        }

        public static void SendToClient<T>(ClientStructure client, IMessageData data)
            where T : IServerMessageBase, new()
        {
            var newMessage = ServerContext.ServerMessageFactory.CreateNew<T>();
            newMessage.SetData(data);

            if (newMessage.Data == null) return;

            client.SendMessageQueue.Enqueue(newMessage);
        }

        public static void SendToClient(ClientStructure client, IServerMessageBase msg)
        {
            if (msg.Data == null) return;
            client.SendMessageQueue.Enqueue(msg);
        }

        public static void SendConnectionEnd(ClientStructure client, string reason)
        {
            ClientConnectionHandler.DisconnectClient(client, reason);
        }

        public static void SendConnectionEndToAll(string reason)
        {
            foreach (var client in ClientRetriever.GetAuthenticatedClients())
                SendConnectionEnd(client, reason);
        }
    }
}