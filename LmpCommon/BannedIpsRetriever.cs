using LmpCommon.Collection;
using LmpCommon.Time;
using LmpGlobal;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LmpCommon
{
    /// <summary>
    /// This class retrieves the banned ips stored in <see cref="RepoConstants.BannedIpListUrl"/>
    /// </summary>
    public static class BannedIpsRetriever
    {
        private static readonly ConcurrentHashSet<IPAddress> PrivBannedIPs = new ConcurrentHashSet<IPAddress>();
        private static ConcurrentHashSet<IPAddress> BannedIps
        {
            get
            {
                if (LunaComputerTime.UtcNow - _lastRequestTime > MaxRequestInterval)
                {
                    Task.Run(() => RefreshBannedIps());
                    _lastRequestTime = LunaComputerTime.UtcNow;
                }

                return PrivBannedIPs;
            }
        }

        private static readonly TimeSpan MaxRequestInterval = TimeSpan.FromMinutes(10);
        private static DateTime _lastRequestTime = DateTime.MinValue;
        
        public static bool IsBanned(IPEndPoint endpoint)
        {
            return BannedIps.Contains(endpoint.Address);
        }

        /// <summary>
        /// Download the banned ips list from the <see cref="RepoConstants.BannedIpListUrl"/> and return the ones that are correctly written
        /// </summary>
        private static void RefreshBannedIps()
        {
            BannedIps.Clear();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = GithubCertification.MyRemoteCertificateValidationCallback;
                using (var client = new WebClient())
                using (var stream = client.OpenRead(RepoConstants.BannedIpListUrl))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var content = reader.ReadToEnd();
                        var ips = content
                            .Trim().Split('\n')
                            .Where(s => !s.StartsWith("#") && !string.IsNullOrEmpty(s)).ToArray();

                        foreach (var ip in ips)
                        {
                            try
                            {
                                if (!IPAddress.TryParse(ip, out var ipAddr))
                                {
                                    PrivBannedIPs.Add(ipAddr);
                                }
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
