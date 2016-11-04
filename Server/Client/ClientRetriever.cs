using System.Linq;
using System.Net;
using LunaServer.Context;

namespace LunaServer.Client
{
    public class ClientRetriever
    {
        public static int GetActiveClientCount()
        {
            return GetAuthenticatedClients().Length;
        }

        public static string GetActivePlayerNames()
        {
            var playerString = string.Join(", ", GetAuthenticatedClients().Select(p => p.PlayerName).ToArray());
            return playerString;
        }

        public static ClientStructure[] GetClients()
        {
            return ServerContext.Clients.Values.ToArray();
        }

        public static ClientStructure[] GetAuthenticatedClients()
        {
            return ServerContext.Clients.Values.Where(c => c.Authenticated).ToArray();
        }

        public static bool ClientConnected(ClientStructure client)
        {
            return ServerContext.Clients.ContainsKey(client.Endpoint);
        }

        public static ClientStructure GetClientByName(string playerName)
        {
            return GetAuthenticatedClients().FirstOrDefault(testClient => testClient.PlayerName == playerName);
        }

        public static ClientStructure GetClientByIp(IPAddress ipAddress)
        {
            return GetAuthenticatedClients().FirstOrDefault(testClient => Equals(testClient.Endpoint.Address, ipAddress));
        }

        public static ClientStructure GetClientByPublicKey(string publicKey)
        {
            return GetAuthenticatedClients().FirstOrDefault(testClient => testClient.PublicKey == publicKey);
        }
    }
}