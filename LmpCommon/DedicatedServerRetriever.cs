using LmpCommon.Collection;
using LmpCommon.Time;
using LmpGlobal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LmpCommon
{
    /// <summary>
    /// This class retrieves the dedicated servers stored in
    /// <see cref="RepoConstants.DedicatedServersListUrl"/>
    /// </summary>
    public static class DedicatedServerRetriever
    {
        public static ConcurrentHashSet<string> DedicatedServers
        {
            get
            {
                if (LunaComputerTime.UtcNow - _lastRequestTime > MaxRequestInterval)
                {
                    _lastRequestTime = LunaComputerTime.UtcNow;
                    Task.Run(() => RefreshDedicatedServersList());
                }

                return MasterServerEndpoints;
            }
        }

        private static readonly TimeSpan MaxRequestInterval = TimeSpan.FromMinutes(30);
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly ConcurrentHashSet<string> MasterServerEndpoints = new ConcurrentHashSet<string>();

        public static bool IsDedicatedServer(string endpoint)
        {
            return DedicatedServers.Contains(endpoint);
        }

        public static bool IsDedicatedServer(IPEndPoint endpoint)
        {
            return DedicatedServers.Contains(Common.StringFromEndpoint(endpoint));
        }

        /// <summary>
        /// Download the dedicated server list from the <see cref="RepoConstants.DedicatedServersListUrl"/> and return the ones that are correctly written
        /// </summary>
        private static void RefreshDedicatedServersList()
        {
            MasterServerEndpoints.Clear();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = GithubCertification.MyRemoteCertificateValidationCallback;
                var parsedServers = new List<IPEndPoint>();
                using (var client = new WebClient())
                using (var stream = client.OpenRead(RepoConstants.DedicatedServersListUrl))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var content = reader.ReadToEnd();
                        var servers = content
                            .Trim()
                            .Split('\n')
                            .Where(s => !s.StartsWith("#") && s.Contains(":") && !string.IsNullOrEmpty(s))
                            .ToArray();

                        foreach (var server in servers)
                        {
                            try
                            {
                                var ipPort = server.Split(':');
                                if (!IPAddress.TryParse(ipPort[0], out var ip))
                                {
                                    ip = Common.GetIpFromString(ipPort[0]);
                                }

                                if (ip != null && ushort.TryParse(ipPort[1], out var port))
                                {
                                    parsedServers.Add(new IPEndPoint(ip, port));
                                }
                            }
                            catch (Exception)
                            {
                                //Ignore the bad server   
                            }
                        }
                    }
                }

#if DEBUG
                parsedServers.Add(new IPEndPoint(IPAddress.Loopback, 8700));
#endif
                foreach (var endpoint in parsedServers.Select(s => $"{s.Address.ToString()}:{s.Port}"))
                    MasterServerEndpoints.Add(endpoint);
            }
            catch (Exception)
            {
                //Ignored
            }
        }
    }
}
