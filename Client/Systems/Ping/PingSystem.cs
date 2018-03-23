using LunaClient.Base;
using LunaClient.Network;
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

        private static ConcurrentBag<string> PingQueue { get; } = new ConcurrentBag<string>();
        protected override bool AlwaysEnabled => true;
        public override string SystemName { get; } = nameof(PingSystem);

        #endregion

        #region Public methods

        public static void QueuePing(string endpoint)
        {
            PingQueue.Add(endpoint);
        }
        
        #endregion

        #region Update methods

        private static void PerformPings()
        {
            while (PingQueue.TryTake(out var endpoint))
            {
                MainSystem.Singleton.StartCoroutine(PingUpdate(endpoint));
            }
        }

        #endregion

        #region Private methods
        
        private static IEnumerator PingUpdate(string endpoint)
        {
            var host = endpoint.Substring(0, endpoint.LastIndexOf(":"));
            var ping = new UnityEngine.Ping(host);

            var elapsedSecs = 0;

            do
            {
                elapsedSecs++;
                yield return new WaitForSeconds(1f);
            } while (!ping.isDone && elapsedSecs < PingTimeoutInSec);

            if (NetworkServerList.Servers.TryGetValue(endpoint, out var server))
            {
                server.Ping = ping.isDone ? ping.time : int.MaxValue;
                server.DisplayedPing = ping.isDone ? ping.time.ToString() : "∞";
            }
        }

        #endregion
    }
}
