using LunaClient.Base;
using LunaClient.Network;
using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.Ping
{
    public class PingSystem : Base.System
    {
        /// <summary>
        /// This system must be ALWAYS enabled!
        /// </summary>
        public override bool Enabled => true;

        public override string SystemName { get; } = nameof(PingSystem);

        #region Fields & properties

        private static ConcurrentBag<string> PingQueue { get; } = new ConcurrentBag<string>();

        #endregion

        public PingSystem()
        {
            //We setup the routine in the constructor as this system is always enabled
            SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, PerformPings));
        }

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
                Client.Singleton.StartCoroutine(PingUpdate(endpoint));
            }
        }

        #endregion

        #region Private methods
        
        private static IEnumerator PingUpdate(string endpoint)
        {
            var host = endpoint.Substring(0, endpoint.LastIndexOf(":"));
            var ping = new UnityEngine.Ping(host);

            yield return new WaitForSeconds(2f);

            var pingTime = ping.isDone ? ping.time : 9999;
            if (NetworkServerList.Servers.TryGetValue(endpoint, out var server))
            {
                server.Ping = pingTime;
            }
        }

        #endregion
    }
}
