using LmpClient.Base;
using LmpClient.Network;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private static readonly HashSet<long> RunningPings = new HashSet<long>();

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
                if (!RunningPings.Contains(serverId))
                {
                    RunningPings.Add(serverId);
                    MainSystem.Singleton.StartCoroutine(PingUpdate(serverId));
                }
            }
        }

        #endregion

        #region Private methods

        private static IEnumerator PingUpdate(long serverId)
        {
            if (NetworkServerList.Servers.TryGetValue(serverId, out var serverInfo))
            {
                var host = serverInfo.ExternalEndpoint.Address;

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
                    server.Ping = result;
                    server.DisplayedPing = finished ? result.ToString() : "∞";
                }

                RunningPings.Remove(serverId);
            }
        }

        #endregion
    }
}
