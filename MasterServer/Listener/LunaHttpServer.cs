using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MasterServer.Listener
{
    public class LunaHttpServer
    {
        private static TcpListener _listener;

        private static Dictionary<IPAddress, DateTime> FloodControl = new Dictionary<IPAddress, DateTime>();

        public void Listen(int port)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _listener.Start();
            while (MasterServer.RunServer)
            {
                var client = _listener.AcceptTcpClient();
                var clientEndpoint = ((IPEndPoint)client.Client.RemoteEndPoint).Address;

                //We only allow requests from the same client every 10 seconds
                if (FloodControl.TryGetValue(clientEndpoint, out var lastRequest) && (DateTime.UtcNow - lastRequest).TotalSeconds < 10)
                {
                    Logger.Log(LogLevels.Debug, $"Ignoring GET request from {client.Client.RemoteEndPoint}");
                    client.Close();
                    continue;
                }

                Logger.Log(LogLevels.Debug, $"Received a GET request from {client.Client.RemoteEndPoint}");

                var processor = new HttpProcessor(client, this);
                Task.Run(() => processor.Process());

                AddClientToFloodControl(clientEndpoint);

                Thread.Sleep(MasterServer.ServerMsTick);
            }
        }

        private static void AddClientToFloodControl(IPAddress clientEndpoint)
        {
            if (FloodControl.ContainsKey(clientEndpoint))
                FloodControl[clientEndpoint] = DateTime.UtcNow;
            else
                FloodControl.Add(clientEndpoint, DateTime.UtcNow);
        }

        public static void HandleGetRequest(HttpProcessor p)
        {
            if (p.HttpUrl.Equals("/Test.png"))
            {
                Stream fs = File.Open("../../Test.png", FileMode.Open);

                p.WriteSuccess("image/png");
                fs.CopyTo(p.OutputStream.BaseStream);
                p.OutputStream.BaseStream.Flush();
            }
            
            p.WriteSuccess();

            var serializedObjects = Newtonsoft.Json.JsonConvert.SerializeObject(MasterServer.ServerDictionary.Values.Select(s=> s.Info));
            p.OutputStream.Write(serializedObjects);
        }
    }
}
