using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MasterServer.Http
{
    public class HttpProcessor
    {
        public TcpClient Socket;
        public LunaHttpServer Srv;

        private Stream _inputStream;
        public StreamWriter OutputStream;

        public string HttpMethod;
        public string HttpUrl;
        public string HttpProtocolVersionstring;
        public Hashtable HttpHeaders = new Hashtable();

        public HttpProcessor(TcpClient s, LunaHttpServer srv)
        {
            Socket = s;
            Srv = srv;
        }

        private static string StreamReadLine(Stream inputStream)
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
                        Thread.Sleep(MasterServer.ServerMsTick); continue;
                }
                ;
                data += Convert.ToChar(nextChar);
            }
            return data;
        }

        public void Process()
        {
            // we can't use a StreamReader for input, because it buffers up extra data on us inside it's
            // "processed" view of the world, and we want the data raw after the headers
            _inputStream = new BufferedStream(Socket.GetStream());

            // we probably shouldn't be using a streamwriter for all output from handlers either
            OutputStream = new StreamWriter(new BufferedStream(Socket.GetStream()));
            try
            {
                ParseRequest();
                ReadHeaders();
                if (HttpMethod.Equals("GET"))
                {
                    HandleGetRequest();
                }
            }
            catch (Exception)
            {
                WriteFailure();
            }
            OutputStream.Flush();
            _inputStream = null; OutputStream = null; // bs = null;            
            Socket.Close();
        }

        public void ParseRequest()
        {
            var request = StreamReadLine(_inputStream);
            var tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                //invalid http request line
                return;
            }
            HttpMethod = tokens[0].ToUpper();
            HttpUrl = tokens[1];
            HttpProtocolVersionstring = tokens[2];
        }

        public void ReadHeaders()
        {
            string line;
            while ((line = StreamReadLine(_inputStream)) != null)
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

        public void HandleGetRequest()
        {
            LunaHttpServer.HandleGetRequest(this);
        }

        public void WriteSuccess(string contentType = "text/html")
        {
            OutputStream.WriteLine("HTTP/1.0 200 OK");
            OutputStream.WriteLine("Content-Type: " + contentType);
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }

        public void WriteFailure()
        {
            OutputStream.WriteLine("HTTP/1.0 404 File not found");
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }
    }
}
