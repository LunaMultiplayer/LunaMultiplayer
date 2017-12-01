using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer.Http
{
    public class LunaHttpServer
    {
        public static ushort Port { get; set; } = 8701;

        private static readonly TcpListener Listener = new TcpListener(IPAddress.Any, Port);

        public void Listen()
        {
            Listener.Start();
            while (MasterServer.RunServer)
            {
                var client = Listener.AcceptTcpClient();
                var clientEndpoint = ((IPEndPoint) client.Client.RemoteEndPoint).Address;

                //We only allow requests from the same client every 10 seconds
                if (!FloodControl.AllowRequest(clientEndpoint))
                {
                    Logger.Log(LogLevels.Debug, $"Ignoring GET request from {client.Client.RemoteEndPoint}");
                    client.Close();
                    continue;
                }

                Logger.Log(LogLevels.Debug, $"Received a GET request from {client.Client.RemoteEndPoint}");

                var processor = new HttpProcessor(client, this);
                Task.Run(() => processor.Process());

                Thread.Sleep(MasterServer.ServerMsTick);
            }
        }

        public static void HandleGetRequest(HttpProcessor p)
        {
            p.WriteSuccess();
            if (!string.IsNullOrEmpty(p.JQueryCallBack))
            {
                p.OutputStream.Write($"{p.JQueryCallBack}({Newtonsoft.Json.JsonConvert.SerializeObject(MasterServer.ServerDictionary.Values.Select(s => s.Info))})");
            }
            else
            {
                p.OutputStream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(MasterServer.ServerDictionary.Values.Select(s => s.Info)));
            }
        }
    }
}
