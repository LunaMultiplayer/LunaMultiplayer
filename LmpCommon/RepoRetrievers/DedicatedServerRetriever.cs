using LmpCommon.Collection;
using LmpCommon.Time;
using LmpGlobal;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LmpCommon.RepoRetrievers
{
    /// <summary>
    /// This class retrieves the dedicated servers stored in
    /// <see cref="RepoConstants.DedicatedServersListUrl"/>
    /// </summary> 
    public static class DedicatedServerRetriever
    {
        private static readonly ConcurrentHashSet<IPEndPoint> DedicatedServerEndpoints = new ConcurrentHashSet<IPEndPoint>();
        private static ConcurrentHashSet<IPEndPoint> DedicatedServers
        {
            get
            {
                if (_lastRequestTime == DateTime.MinValue)
                {
                    //Run syncronously if it's the first time
                    RefreshDedicatedServersList();
                    _lastRequestTime = LunaComputerTime.UtcNow;
                }
                else if (LunaComputerTime.UtcNow - _lastRequestTime > MaxRequestInterval)
                {
                    Task.Run(() => RefreshDedicatedServersList());
                    _lastRequestTime = LunaComputerTime.UtcNow;
                }

                return DedicatedServerEndpoints;
            }
        }

        private static readonly TimeSpan MaxRequestInterval = TimeSpan.FromMinutes(30);
        private static DateTime _lastRequestTime = DateTime.MinValue;

        public static bool IsDedicatedServer(IPEndPoint endpoint)
        {
            return DedicatedServers.Contains(endpoint);
        }

        /// <summary>
        /// Download the dedicated server list from the <see cref="RepoConstants.DedicatedServersListUrl"/> and return the ones that are correctly written
        /// </summary>
        private static void RefreshDedicatedServersList()
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = GithubCertification.MyRemoteCertificateValidationCallback;
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

                        DedicatedServerEndpoints.Clear();

                        foreach (var server in servers)
                        {
                            try
                            {
                                DedicatedServerEndpoints.Add(LunaNetUtils.CreateEndpointFromString(server));
                            }
                            catch (Exception)
                            {
                                //Ignore the bad server   
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                //Ignored
            }
        }
    }
}
