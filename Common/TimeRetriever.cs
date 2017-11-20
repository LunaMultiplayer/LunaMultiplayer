using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace LunaCommon
{
    public class TimeRetriever
    {
        //Server addresses and information from //http://tf.nist.gov/tf-cgi/servers.cgi
        public const string Server = "time.nist.gov";
        public const int Port = 13;

        private static readonly TcpClient TcpClientConnection = new TcpClient();
        private static NetworkStream _netStream;

        private static DateTime _lastRequest = DateTime.MinValue;

        public static DateTime GetNistTime()
        {
            //Max requests are every 4 seconds
            if ((DateTime.UtcNow - _lastRequest).TotalSeconds > 4)
            {
                _lastRequest = DateTime.UtcNow;
                var dateTime = GetNistDateTimeFromSocket();
                return dateTime == DateTime.MinValue ? GetNistTimeFromWeb() : DateTime.MinValue;
            }

            return DateTime.MinValue;
        }

        internal static DateTime GetNistDateTimeFromSocket()
        {
            if (ConnectToNist())
            {
                using (_netStream = TcpClientConnection.GetStream())
                {
                    if (_netStream.CanRead)
                    {
                        var bytes = new byte[TcpClientConnection.ReceiveBufferSize];
                        _netStream.Read(bytes, 0, TcpClientConnection.ReceiveBufferSize);
                        CleanUpStreams();
                        
                        var timeAsString = Encoding.ASCII.GetString(bytes).Substring(0, 50);
                        return ParseStringTime(timeAsString);
                    }
                }
            }

            CleanUpStreams();
            return DateTime.MinValue;
        }

        internal static DateTime GetNistTimeFromWeb()
        {
            var dateTime = DateTime.MinValue;

            var request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi?lzbc=siqm9b");
            request.Method = "GET";
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching

            var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                using (var stream = new StreamReader(response.GetResponseStream()))
                {
                    var html = stream.ReadToEnd(); //<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
                    var time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
                    var milliseconds = Convert.ToInt64(time) / 1000.0;
                    dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds);
                }
            }

            return dateTime;
        }

        private static void CleanUpStreams()
        {
            TcpClientConnection.Close();
            _netStream.Close();
        }

        private static DateTime ParseStringTime(string timeAsString)
        {
            try
            {
                return DateTime.Parse("20" + timeAsString.Substring(7, 17));
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static bool ConnectToNist()
        {
            var connectionTries = 0;
            while (!TcpClientConnection.Connected && connectionTries < 5)
            {
                try
                {
                    TcpClientConnection.Connect(Server, Port);
                    connectionTries++;
                }
                catch
                {
                    // ignored
                }
            }

            return TcpClientConnection.Connected;
        }
    }
}
