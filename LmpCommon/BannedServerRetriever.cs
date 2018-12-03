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
    /// This class retrieves the banned servers stored in
    /// <see cref="RepoConstants.BannedServersListUrl"/>
    /// </summary>
    public static class BannedServerRetriever
    {
        public static ConcurrentHashSet<string> BannedServers
        {
            get
            {
                if (LunaComputerTime.UtcNow - _lastRequestTime > MaxRequestInterval)
                {
                    Task.Run(() => RefreshBannedServersList());
                    _lastRequestTime = LunaComputerTime.UtcNow;
                }

                return BannedServerEndpoints;
            }
        }

        private static readonly TimeSpan MaxRequestInterval = TimeSpan.FromMinutes(10);
        private static DateTime _lastRequestTime = DateTime.MinValue;
        private static readonly ConcurrentHashSet<string> BannedServerEndpoints = new ConcurrentHashSet<string>();

        public static bool IsBannedServer(string endpoint)
        {
            return BannedServers.Contains(endpoint);
        }

        public static bool IsBannedServer(IPEndPoint endpoint)
        {
            return BannedServers.Contains(Common.StringFromEndpoint(endpoint));
        }

        /// <summary>
        /// Download the banned server list from the <see cref="RepoConstants.BannedServersListUrl"/> and return the ones that are correctly written
        /// </summary>
        private static void RefreshBannedServersList()
        {
            BannedServerEndpoints.Clear();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = GithubCertification.MyRemoteCertificateValidationCallback;
                var parsedServers = new List<IPEndPoint>();
                using (var client = new WebClient())
                using (var stream = client.OpenRead(RepoConstants.BannedServersListUrl))
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

                foreach (var endpoint in parsedServers.Select(s => $"{s.Address.ToString()}:{s.Port}"))
                    BannedServerEndpoints.Add(endpoint);
            }
            catch (Exception)
            {
                //Ignored
            }
        }
    }
}
