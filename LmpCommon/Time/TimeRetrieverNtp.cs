using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LmpCommon.Time
{
    internal static class TimeRetrieverNtp
    {
        private const int DaysTo1900 = 1900 * 365 + 95; // 95 = offset for leap-years etc.
        private const long TicksTo1900 = DaysTo1900 * TimeSpan.TicksPerDay;

        private const byte NtpDataLength = 48;
        private static byte[] _ntpData = new byte[NtpDataLength];
        private static IPEndPoint _serverAddress;

        internal static DateTime GetNtpTime(string server)
        {
            InitializeStructure();

            _serverAddress = new IPEndPoint(Dns.GetHostEntry(server).AddressList[0], 123);

            // ReSharper disable once RedundantAssignment
            var pingDuration = Stopwatch.GetTimestamp(); // temp access (JIT-Compiler need some time at first call)

            using (var socket = new UdpClient(_serverAddress.AddressFamily))
            {
                socket.Connect(_serverAddress);
                socket.Send(_ntpData, _ntpData.Length);

                pingDuration = Stopwatch.GetTimestamp(); // after Send-Method to reduce WinSocket API-Call time
                _ntpData = socket.Receive(ref _serverAddress);
                pingDuration = Stopwatch.GetTimestamp() - pingDuration;
            }

            var pingTicks = pingDuration * TimeSpan.TicksPerSecond / Stopwatch.Frequency;

            // optional: display response-time
            // Console.WriteLine("{0:N2} ms", new TimeSpan(pingTicks).TotalMilliseconds);

            var intPart = (long)_ntpData[40] << 24 | (long)_ntpData[41] << 16 | (long)_ntpData[42] << 8 | _ntpData[43];
            var fractPart = (long)_ntpData[44] << 24 | (long)_ntpData[45] << 16 | (long)_ntpData[46] << 8 | _ntpData[47];
            var netTicks = intPart * TimeSpan.TicksPerSecond + (fractPart * TimeSpan.TicksPerSecond >> 32);

            var networkDateTime = new DateTime(TicksTo1900 + netTicks + pingTicks / 2);

            return networkDateTime;
        }

        private static void InitializeStructure()
        {
            _ntpData[0] = 0x1B; // LeapIndicator = 0 (no warning), VersionNum = 3 (IPv4 only), Mode = 3 (Client Mode)
            for (var i = 1; i < 48; i++) _ntpData[i] = 0;
        }
    }
}
