using System.Collections.Concurrent;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.KerbalSys
{
    public class KerbalMessageHandler : SubSystem<KerbalSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as KerbalReplyMsgData;
            if (msgData == null) return;

            HandleKerbalReply(msgData);
        }

        private static void HandleKerbalReply(KerbalReplyMsgData messageData)
        {
            var planetTime = messageData.PlanetTime;

            foreach (var kerbal in messageData.KerbalsData)
            {
                var kerbalNode = ConfigNodeSerializer.Singleton.Deserialize(kerbal.Value);
                if (kerbalNode != null)
                    QueueKerbal(planetTime, kerbal.Key, kerbalNode);
                else
                    Debug.LogError("Failed to load kerbal!");
            }

            Debug.Log("Kerbals Synced!");
            MainSystem.Singleton.NetworkState = ClientState.KERBALS_SYNCED;
        }

        private static void QueueKerbal(double planetTime, string kerbalName, ConfigNode kerbalNode)
        {
            var newEntry = new KerbalEntry
            {
                PlanetTime = planetTime,
                KerbalNode = kerbalNode
            };

            if (!System.KerbalProtoQueue.ContainsKey(kerbalName))
                System.KerbalProtoQueue.Add(kerbalName, new Queue<KerbalEntry>());

            var keQueue = System.KerbalProtoQueue[kerbalName];
            keQueue.Enqueue(newEntry);
        }
    }
}
