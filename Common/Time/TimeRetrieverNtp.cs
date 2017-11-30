using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LunaCommon.Time
{
    internal static class TimeRetrieverNtp
    {
        internal static DateTime GetNtpTime(string server, bool getAsLocalTime)
        {
            const int daysTo1900 = 1900 * 365 + 95; // 95 = offset for leap-years etc.
            const long ticksTo1900 = daysTo1900 * TimeSpan.TicksPerDay;

            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(server).AddressList;
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
    }
}
