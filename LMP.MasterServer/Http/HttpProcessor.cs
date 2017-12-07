using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LMP.MasterServer.Http
{
    public class HttpProcessor
    {
        public static string JQueryCallBack;

        public TcpClient Socket;
        
        public string HttpMethod;
        public string HttpUrl;

        public string HttpProtocolVersionstring;
        public Hashtable HttpHeaders = new Hashtable();

        public HttpProcessor(TcpClient s)
        {
            Socket = s;
        }

        private static async Task<string> StreamReadLine(Stream inputStream)
        {
            var data = "";
            while (true)
            {
                var nextChar = inputStream.ReadByte();
                if (nextChar == '\n') { break; }
                switch (nextChar)
                {
                    case '\r':
                        continue;
                    case -1:
                        await Task.Delay(MasterServer.ServerMsTick); continue;
                }
                ;
                data += Convert.ToChar(nextChar);
            }
            return data;
        }

        public void Process()
        {
            using (var inputStream = new BufferedStream(Socket.GetStream()))
            using (var outputStream = new StreamWriter(new BufferedStream(Socket.GetStream())))
            {
                try
                {
                    ParseRequest(inputStream);
                    ReadHeaders(inputStream);
                    if (HttpMethod.Equals("GET"))
                    {
                        HandleGetRequest(outputStream);
                    }
                }
                catch (Exception)
                {
                    WriteFailure(outputStream);
                }
                outputStream.Flush();
                Socket.Close();
                Socket.Dispose();
            }
        }

        public async void ParseRequest(BufferedStream inputStream)
        {
            var request = await StreamReadLine(inputStream);
            var tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                //invalid http request line
                return;
            }
            HttpMethod = tokens[0].ToUpper();
            HttpUrl = tokens[1];
            JQueryCallBack = HttpUrl.Contains("jQuery") ? HttpUrl.Substring(0, HttpUrl.LastIndexOf("&")).Substring(HttpUrl.IndexOf("jQuery")) : string.Empty;
            HttpProtocolVersionstring = tokens[2];
        }

        public async void ReadHeaders(BufferedStream inputStream)
        {
            string line;
            while ((line = await StreamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    return;
                }

                var separator = line.IndexOf(':');
                if (separator == -1)
                {
                    return;
                }

                var name = line.Substring(0, separator);
                var pos = separator + 1;
                while (pos < line.Length && line[pos] == ' ')
                {
                    pos++; // strip any spaces
                }

                var value = line.Substring(pos, line.Length - pos);
                HttpHeaders[name] = value;
            }
        }

        public void HandleGetRequest(StreamWriter outputStream)
        {
            LunaHttpServer.HandleGetRequest(outputStream);
        }
        
        public void WriteFailure(StreamWriter outputStream)
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
        }
    }
}
