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

        #region Fields & properties

        private static ConcurrentQueue<string> PingQueue { get; } = new ConcurrentQueue<string>();

        #endregion

        public PingSystem()
        {
            //We setup the routine in the constructor as this system is always enabled
            SetupRoutine(new RoutineDefinition(100, RoutineExecution.Update, PerformPings));
        }

        #region Public methods

        public static void QueuePing(string host)
        {
            PingQueue.Enqueue(host);
        }
        
        #endregion

        #region Update methods

        private static void PerformPings()
        {
            while (PingQueue.TryDequeue(out var host))
            {
                Client.Singleton.StartCoroutine(PingUpdate(host));
            }
        }

        #endregion

        #region Private methods
        
        private static IEnumerator PingUpdate(string host)
        {
            var ping = new UnityEngine.Ping(host);

            yield return new WaitForSeconds(2f);

            var pingTime = ping.isDone ? ping.time : 9999;
            if (NetworkServerList.Servers.TryGetValue(host, out var server))
            {
                server.Ping = pingTime;
            }
        }

        #endregion
    }
}
