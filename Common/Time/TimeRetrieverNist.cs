using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;

namespace LunaCommon.Time
{
    internal static class TimeRetrieverNist
    {
        internal static DateTime? GetNistTime()
        {
            using (var client = new TcpClient("time.nist.gov", 13))
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                var response = streamReader.ReadToEnd();
                if (string.IsNullOrEmpty(response) && response.Length > 24)
                {
                    return DateTime.ParseExact(response.Substring(7, 17), "yy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                }

                return null;
            }
        }
    }
}
