using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LMP.MasterServer.Http
{
    public class LunaHttpServer
    {
        public static ushort Port { get; set; } = 8701;
        private static readonly TcpListener Listener = new TcpListener(IPAddress.Any, Port);

        public void Listen()
        {
            Listener.Start();
            Task.Run(() => StartListeningAndParsing());
        }

        private static async void StartListeningAndParsing()
        {
            while (MasterServer.RunServer)
            {
                var client = await Listener.AcceptTcpClientAsync();
                await TcpClientWorker.ParseClientRequest(client);
                //var client = Listener.AcceptTcpClientAsync();
                //if (client.IsCompleted)
                //    await TcpClientWorker.ParseClientRequest(client.Result);
            }
            Listener.Stop();
        }

        public static void HandleGetRequest(StreamWriter outputStream)
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: text/html");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");

            if (!string.IsNullOrEmpty(HttpProcessor.JQueryCallBack))
            {
                outputStream.Write($"{HttpProcessor.JQueryCallBack}({Newtonsoft.Json.JsonConvert.SerializeObject(MasterServer.ServerDictionary.Values.Select(s => s.Info))})");
            }
            else
            {
                outputStream.Write(Newtonsoft.Json.JsonConvert.SerializeObject(MasterServer.ServerDictionary.Values.Select(s => s.Info)));
            }
        }


    }
}
