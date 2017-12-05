using LMP.Server.Client;
using LunaCommon.Message.Interface;

namespace LMP.Server.Plugin
{
    public interface ILmpPlugin
    {
        /// <summary>
        ///     Fires every main thread tick (10ms).
        /// </summary>
        void OnUpdate();

        /// <summary>
        ///     Fires just after the server is started or restarted.
        /// </summary>
        void OnServerStart();

        /// <summary>
        ///     Fires just before the server stops or restarts.
        /// </summary>
        void OnServerStop();

        /// <summary>
        ///     Fires when the client's Connection is accepted.
        /// </summary>
        void OnClientConnect(ClientStructure client);

        /// <summary>
        ///     Fires just after the client has Authenticated
        /// </summary>
        void OnClientAuthenticated(ClientStructure client);

        /// <summary>
        ///     Fires when a client disconnects
        /// </summary>
        void OnClientDisconnect(ClientStructure client);

        /// <summary>
        ///     Fires every time a message is received from a client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="messageData">The message payload (Null for certain types)</param>
        void OnMessageReceived(ClientStructure client, IClientMessageBase messageData);

        /// <summary>
        ///     Fires every time a message is sent to a client
        /// </summary>
        /// <param name="client">The client that has sent the message</param>
        /// <param name="messageData">The message payload (Null for certain types)</param>
        void OnMessageSent(ClientStructure client, IServerMessageBase messageData);
    }
}