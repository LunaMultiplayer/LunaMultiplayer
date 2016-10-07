using System;
using System.Threading.Tasks;
using LunaCommon.Enums;
using LunaServer.Log;

namespace LunaServer.Client
{
    public class ClientException
    {
        public static void HandleDisconnectException(string location, ClientStructure client, Exception e)
        {
            if (!client.DisconnectClient && (client.ConnectionStatus != ConnectionStatus.DISCONNECTED))
                if (e.InnerException != null)
                    LunaLog.Normal(
                        $"Client {client.PlayerName} disconnected in {location}, Endpoint {client.Endpoint}, " +
                        $"error: {e.Message} ({e.InnerException.Message})");
                else
                    LunaLog.Normal(
                        $"Client {client.PlayerName} disconnected in {location}, Endpoint {client.Endpoint}, " +
                        $"error: {e.Message}");
            ClientConnectionHandler.DisconnectClient(client);
        }
    }
}