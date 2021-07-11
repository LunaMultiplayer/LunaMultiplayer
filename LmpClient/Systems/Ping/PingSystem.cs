using LmpClient.Base;
using LmpClient.Network;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace LmpClient.Systems.Ping
{
    public class PingSystem : System<PingSystem>
    {
        #region Constructor

        /// <inheritdoc />
        /// <summary>
        /// We setup the routine in the constructor as this system is always enabled
        /// </summary>
        public PingSystem() => SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, PerformPings));

        #endregion

        #region Fields & properties

        private const float PingTimeoutInSec = 7.5f;

        private static readonly HashSet<(long, bool)> RunningPings = new HashSet<(long, bool)>();

        private static ConcurrentBag<long> PingQueue { get; } = new ConcurrentBag<long>();
        protected override bool AlwaysEnabled => true;
        public override string SystemName { get; } = nameof(PingSystem);

        #endregion

        #region Public methods

        public static void QueuePing(long id)
        {
            PingQueue.Add(id);
        }

        #endregion

        #region Update methods

        private static void PerformPings()
        {
            while (PingQueue.TryTake(out var serverId))
            {
                foreach (var ipv6 in new []{true, false})
                {
                    if (!RunningPings.Contains((serverId, ipv6)))
                    {
                        RunningPings.Add((serverId, ipv6));
                        MainSystem.Singleton.StartCoroutine(PingUpdate(serverId, ipv6));
                    }
                }
            }
        }

        #endregion

        #region Private methods

        private static IEnumerator PingUpdate(long serverId, bool ipv6)
        {
            if (NetworkServerList.Servers.TryGetValue(serverId, out var serverInfo))
            {
                IPAddress host;
                if (ipv6)
                {
                    host = serverInfo.InternalEndpoint6.Address;
                    if (host.Equals(IPAddress.IPv6Loopback))
                    {
                        serverInfo.Ping6 = int.MaxValue;
                        serverInfo.DisplayedPing6 = "X";
                        RunningPings.Remove((serverId, ipv6));
                        yield break;
                    }
                }
                else
                {
                    host = serverInfo.ExternalEndpoint.Address;
                    if (host.Equals(IPAddress.Loopback))
                    {
                        serverInfo.Ping = int.MaxValue;
                        serverInfo.DisplayedPing = "X";
                        RunningPings.Remove((serverId, ipv6));
                        yield break;
                    }
                }

                var ping = new UnityEngine.Ping(host.ToString());
                var elapsedSecs = 0f;

                while (!ping.isDone && elapsedSecs < PingTimeoutInSec)
                {
                    yield return null;
                    elapsedSecs += Time.deltaTime;
                }

                var finished = ping.isDone;
                var result = finished ? ping.time : int.MaxValue;
                ping.DestroyPing();

                if (NetworkServerList.Servers.TryGetValue(serverId, out var server))
                {
                    if (ipv6)
                    {
                        server.Ping6 = result;
                        server.DisplayedPing6 = finished ? result.ToString() : "∞";
                    }
                    else
                    {
                        server.Ping = result;
                        server.DisplayedPing = finished ? result.ToString() : "∞";
                    }
                }

                RunningPings.Remove((serverId, ipv6));
            }
        }

        #endregion
    }
}
