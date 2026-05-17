using LunaConfigNode.CfgNode;
using Server.Log;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;

namespace Server.System.Scenario
{
    public partial class ScenarioDataUpdater
    {
        #region Semaphore

        /// <summary>
        /// To not overwrite our own data we use a lock
        /// </summary>
        private static readonly ConcurrentDictionary<string, object> Semaphore = new ConcurrentDictionary<string, object>();

        #endregion

        /// <summary>
        /// Creates a ConfigNode from raw bytes, stripping the outer { } braces that KSP's
        /// ConfigNode.WriteNode() adds. LunaConfigNode's parser wraps braced content in an
        /// unnamed child node, which causes GetValue() on the root to return null.
        /// </summary>
        private static ConfigNode ParseClientConfigNode(byte[] data, int numBytes, string nodeName)
        {
            var raw = Encoding.UTF8.GetString(data, 0, numBytes);
            var trimmed = raw.Trim();

            // KSP serializes unnamed ConfigNodes as "{\n\tkey = val\n}" — strip the wrapper
            if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                trimmed = trimmed.Substring(1, trimmed.Length - 2);

            return new ConfigNode(trimmed) { Name = nodeName };
        }

        /// <summary>
        /// Raw updates a scenario in the dictionary, stripping outer { } braces
        /// that KSP's ConfigNode serializer adds (same fix as ParseClientConfigNode).
        /// </summary>
        public static void RawConfigNodeInsertOrUpdate(string scenarioModule, string scenarioAsConfigNode)
        {
            _ = Task.Run(() =>
            {
                var trimmed = scenarioAsConfigNode.Trim();
                if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
                    trimmed = trimmed.Substring(1, trimmed.Length - 2);

                var scenario = new ConfigNode(trimmed) { Name = scenarioModule };
                lock (Semaphore.GetOrAdd(scenarioModule, new object()))
                {
                    ScenarioStoreSystem.CurrentScenarios.AddOrUpdate(scenarioModule, scenario, (key, existingVal) => scenario);
                }
            });
        }
    }
}
