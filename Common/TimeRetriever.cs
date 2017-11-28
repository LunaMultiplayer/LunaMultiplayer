using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace LunaCommon
{
    /// <summary>
    /// Use this class to retrieve exact times
    /// </summary>
    public class LunaTime
    {
        /// <summary>
        /// Get correctly sync UTC time from internet
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                if (_timeDifference == null)
                {
                    _timeDifference = DateTime.UtcNow - TimeRetriever.GetNtpTime();
                }
                return DateTime.UtcNow - _timeDifference.Value;
            }
        }

        /// <summary>
        /// Get correctly sync local time from internet
        /// </summary>
        public static DateTime Now => UtcNow.ToLocalTime();

        private static TimeSpan? _timeDifference;
    }

    /// <summary>
    /// This class retrieves the exact time from internet
    /// </summary>
    internal class TimeRetriever
    {
        private const string Server = "pool.ntp.org";
        private static DateTime _lastRequest = DateTime.MinValue;

        internal static DateTime GetNtpTime()
        {
            //Max requests are every 4 seconds
            if ((DateTime.UtcNow - _lastRequest).TotalSeconds < 4)
                throw new Exception("Too many time requests!");

            var dateTime = GetNtpTimeFromSocket() ?? GetNistTimeFromWeb();
            _lastRequest = DateTime.UtcNow;

            return dateTime;
        }

        private static DateTime? GetNtpTimeFromSocket(bool getAsLocalTime = false)
        {
            try
            {
                const int daysTo1900 = 1900 * 365 + 95; // 95 = offset for leap-years etc.
                const long ticksTo1900 = daysTo1900 * TimeSpan.TicksPerDay;

                var ntpData = new byte[48];
                ntpData[0] = 0x1B; // LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

                var addresses = Dns.GetHostEntry(Server).AddressList;
                var ipEndPoint = new IPEndPoint(addresses[0], 123);

                // ReSharper disable once RedundantAssignment
                var pingDuration = Stopwatch.GetTimestamp(); // temp access (JIT-Compiler need some time at first call)

                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    socket.Connect(ipEndPoint);
                    socket.ReceiveTimeout = 5000;
                    socket.Send(ntpData);
                    pingDuration = Stopwatch.GetTimestamp(); // after Send-Method to reduce WinSocket API-Call time

                    socket.Receive(ntpData);
                    pingDuration = Stopwatch.GetTimestamp() - pingDuration;
                }

                var pingTicks = pingDuration * TimeSpan.TicksPerSecond / Stopwatch.Frequency;

                // optional: display response-time
                // Console.WriteLine("{0:N2} ms", new TimeSpan(pingTicks).TotalMilliseconds);

                var intPart = (long)ntpData[40] << 24 | (long)ntpData[41] << 16 | (long)ntpData[42] << 8 | ntpData[43];
                var fractPart = (long)ntpData[44] << 24 | (long)ntpData[45] << 16 | (long)ntpData[46] << 8 | ntpData[47];
                var netTicks = intPart * TimeSpan.TicksPerSecond + (fractPart * TimeSpan.TicksPerSecond >> 32);

                var networkDateTime = new DateTime(ticksTo1900 + netTicks + pingTicks / 2);

                // without ToLocalTime() = faster
                return getAsLocalTime ? networkDateTime.ToLocalTime() : networkDateTime;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static DateTime GetNistTimeFromWeb()
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
    }
}