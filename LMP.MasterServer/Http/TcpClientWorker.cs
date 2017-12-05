using LunaCommon;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LMP.MasterServer.Http
{
    class TcpClientWorker
    {

        public static async Task ParseClientRequest(TcpClient client)
        {
            try
            {
                var clientEndpoint = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

                //We only allow requests from the same client every 10 seconds
                if (!FloodControl.AllowRequest(clientEndpoint))
                {
                    ConsoleLogger.Log(LogLevels.Debug, $"Ignoring GET request from {client.Client.RemoteEndPoint}");
                    client.Close();
                    return;
                }

                ConsoleLogger.Log(LogLevels.Debug, $"Received a GET request from {client.Client.RemoteEndPoint}");

                var processor = new HttpProcessor(client);
                await Task.Run(() => processor.Process());
            }
            finally
            {
                client?.Dispose();
            }
        }
    }
}
