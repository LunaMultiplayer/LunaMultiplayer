using System;
using System.Collections.Concurrent;
using System.IO;

namespace LunaClient.Utilities
{
    public class LunaLog
    {
        public static ConcurrentQueue<string> MessageQueue = new ConcurrentQueue<string>();
        private static readonly object ExternalLogLock = new object();

        public static void Debug(string message)
        {
            //Use MessageQueue if looking for messages that don't normally show up in the log.

            MessageQueue.Enqueue("[" + DateTime.Now + "] LunaMultiPlayer: " + message);

            //UnityEngine.Debug.Log("[" + UnityEngine.Time.realtimeSinceStartup + "] LunaMultiPlayer: " + Message);
        }

        public static void Update()
        {
            string message;
            while (MessageQueue.TryDequeue(out message))
                UnityEngine.Debug.Log(message);
        }

        public static void ExternalLog(string debugText)
        {
            lock (ExternalLogLock)
            {
                using (var sw = new StreamWriter(CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "LMP.log"), true))
                {
                    sw.WriteLine(debugText);
                }
            }
        }
    }
}