using LunaClient.Base;
using LunaClient.Network;
using System;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.Ping
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
                MainSystem.Singleton.StartCoroutine(PingUpdate(serverId));
            }
        }

        #endregion

        #region Private methods
        
        private static IEnumerator PingUpdate(long serverId)
        {
            if (NetworkServerList.Servers.TryGetValue(serverId, out var serverInfo))
            {
                var host = serverInfo.ExternalEndpoint.Substring(0, serverInfo.ExternalEndpoint.LastIndexOf(":", StringComparison.InvariantCulture));
                var ping = new UnityEngine.Ping(host);

                var elapsedSecs = 0;

                do
                {
                    elapsedSecs++;
                    yield return new WaitForSeconds(1f);
                } while (!ping.isDone && elapsedSecs < PingTimeoutInSec);

                if (NetworkServerList.Servers.TryGetValue(serverId, out var server))
                {
                    server.Ping = ping.isDone ? ping.time : int.MaxValue;
                    server.DisplayedPing = ping.isDone ? ping.time.ToString() : "∞";
                }
            }
        }

        #endregion
    }
}
