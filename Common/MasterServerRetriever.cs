using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace LunaCommon
{
    /// <summary>
    /// This class retrieves the ips of master servers that are stored in:
    /// http://raw.githubusercontent.com/DaggerES/LunaMultiPlayer/master/MasterServersList
    /// </summary>
    public static class MasterServerRetriever
    {
        public static string MasterServersListShortUrl => "https://goo.gl/NJqZbc";
        public static string MasterServersListUrl => "http://raw.githubusercontent.com/DaggerES/LunaMultiPlayer/master/MasterServersList";

        public static string[] RetrieveWorkingMasterServersIps()
        {
            var parsedServers = new List<IPEndPoint>();
            using (var client = new WebClient())
            using (var stream = client.OpenRead(MasterServersListUrl))
            {
                if (stream == null)
                {
                    throw new Exception($"Cannot open the master servers list file from: {MasterServersListUrl}");
                }
                using (var reader = new StreamReader(stream))
                {
                    var content = reader.ReadToEnd();
                    var servers = content
                        .Trim()
                        .Split('\n')
                        .Where(s => !s.StartsWith("#") && s.Contains(":"))
                        .ToArray();

                    foreach (var server in servers)
                    {
                        var ipPort = server.Split(':');
                        IPAddress ip;
                        ushort port;
                        if (!IPAddress.TryParse(ipPort[0], out ip))
                        {
                            var hostEntry = Dns.GetHostEntry(ipPort[0]);
                            if (hostEntry.AddressList.Length > 0)
                            {
                                ip = hostEntry.AddressList[0];
                            }
                        }

                        if (ip != null && ushort.TryParse(ipPort[1], out port))
                        {
                            var endpoint = new IPEndPoint(ip, port);
                            if (CheckServerHost(endpoint))
                                parsedServers.Add(endpoint);
                        }
                    }
                }
            }

            return parsedServers.Select(s => s.Address.ToString() + ":" + s.Port).ToArray();
        }

        public static bool CheckServerHost(IPEndPoint endpoint)
        {
            try
            {
                var client = new UdpClient(endpoint);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
