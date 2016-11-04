using System.Linq;
using LunaCommon.Message.Interface;
using LunaServer.Client;
using LunaServer.Context;

namespace LunaServer.Server
{
    public class MessageQueuer
    {
        /// <summary>
        /// Sends a message to all the clients except the one given as parameter that are in the same subspace
        /// </summary>
        public static void RelayMessageToSubspace<T>(ClientStructure exceptClient, IMessageData data) where T : IServerMessageBase, new()
        {
            RelayMessageToSubspace<T>(exceptClient, data, exceptClient.Subspace);
        }

        /// <summary>
        /// Sends a message to all the clients in the given subspace
        /// </summary>
        public static void SendMessageToSubspace<T>(IMessageData data, int subspace) where T : IServerMessageBase, new()
        {
            var newMessage = GenerateMessage<T>(data);

            foreach (var otherClient in ServerContext.Clients.Values.Where(c => c.Subspace == subspace))
                SendToClient(otherClient, newMessage);
        }

        /// <summary>
        /// Sends a message to all the clients except the one given as parameter that are in the subspace given as parameter
        /// </summary>
        public static void RelayMessageToSubspace<T>(ClientStructure exceptClient, IMessageData data, int subspace) where T : IServerMessageBase, new()
        {
            var newMessage = GenerateMessage<T>(data);

            foreach (var otherClient in ServerContext.Clients.Values.Where(c => !Equals(c, exceptClient) && c.Subspace == subspace))
                SendToClient(otherClient, newMessage);
        }

        /// <summary>
        /// Sends a message to all the clients except the one given as parameter
        /// </summary>
        public static void RelayMessage<T>(ClientStructure exceptClient, IMessageData data)
            where T : IServerMessageBase, new()
        {
            var newMessage = GenerateMessage<T>(data);

            foreach (var otherClient in ServerContext.Clients.Values.Where(c => !Equals(c, exceptClient)))
                SendToClient(otherClient, newMessage);
        }

        /// <summary>
        /// Sends a message to all the clients
        /// </summary>
        public static void SendToAllClients<T>(IMessageData data)
            where T : IServerMessageBase, new()
        {
            var newMessage = GenerateMessage<T>(data);

            foreach (var otherClient in ServerContext.Clients.Values)
                SendToClient(otherClient, newMessage);
        }

        /// <summary>
        /// Sends a message to the given client
        /// </summary>
        public static void SendToClient<T>(ClientStructure client, IMessageData data)
            where T : IServerMessageBase, new()
        {
            var newMessage = GenerateMessage<T>(data);

            if (newMessage.Data == null) return;

            client.SendMessageQueue.Enqueue(newMessage);
        }

        /// <summary>
        /// Disconnects the given client
        /// </summary>
        public static void SendConnectionEnd(ClientStructure client, string reason)
        {
            ClientConnectionHandler.DisconnectClient(client, reason);
        }

        /// <summary>
        /// Disconnect all clients
        /// </summary>
        public static void SendConnectionEndToAll(string reason)
        {
            foreach (var client in ClientRetriever.GetAuthenticatedClients())
                SendConnectionEnd(client, reason);
        }

        #region Private

        private static void SendToClient(ClientStructure client, IServerMessageBase msg)
        {
            if (msg.Data == null) return;
            client.SendMessageQueue.Enqueue(msg);
        }

        private static T GenerateMessage<T>(IMessageData data) where T : IServerMessageBase, new()
        {
            var newMessage = ServerContext.ServerMessageFactory.CreateNew<T>();
            newMessage.SetData(data);

            return newMessage;
        }

        #endregion
    }
}