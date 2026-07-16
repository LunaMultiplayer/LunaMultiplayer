using LmpCommon;
using LmpCommon.Collection;
using LmpGlobal;
using LmpMasterServer.Lidgren;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LmpMasterServer.Dedicated
{
    /// <summary>
    /// This class retrieves the dedicated servers stored in
    /// <see cref="RepoConstants.DedicatedServersListUrl"/>
    /// </summary>
    public static class DedicatedServerRetriever
    {
        private static readonly ConcurrentHashSet<IPEndPoint> DedicatedServers = new ConcurrentHashSet<IPEndPoint>();
        private static readonly TimeSpan RequestInterval = TimeSpan.FromMinutes(5);

        private static readonly HttpClient Client = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = GithubCertification.MyRemoteCertificateValidationCallback
        });

        public static bool IsDedicatedServer(IPEndPoint endpoint)
        {
            return DedicatedServers.Contains(endpoint);
        }

        /// <summary>
        /// Download the dedicated server list from the <see cref="RepoConstants.DedicatedServersListUrl"/> and return the ones that are correctly written
        /// </summary>
        public static async Task RefreshDedicatedServersListAsync()
        {
            while (MasterServer.RunServer)
            {
                try
                {
                    var content = await Client.GetStringAsync(RepoConstants.DedicatedServersListUrl).ConfigureAwait(false);
                    var servers = content
                        .Trim()
                        .Split('\n')
                        .Where(s => !s.StartsWith("#") && s.Contains(":") && !string.IsNullOrEmpty(s))
                        .ToArray();

                    DedicatedServers.Clear();

                    foreach (var server in servers)
                    {
                        try
                        {
                            DedicatedServers.Add(LunaNetUtils.CreateEndpointFromString(server));
                        }
                        catch (Exception)
                        {
                            //Ignore the bad server
                        }
                    }
                }
                catch (Exception)
                {
                    //Ignored
                }

                await Task.Delay(RequestInterval).ConfigureAwait(false);
            }
        }
    }
}
