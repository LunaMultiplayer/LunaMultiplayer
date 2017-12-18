using Lidgren.Network;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Net;

namespace Server.Client
{
    public class ClientStructure
    {
        public IPEndPoint Endpoint { get; }
        public Guid Id { get; set; }

        public string ActiveVessel { get; set; } = "";
        public bool Authenticated { get; set; }

        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public byte[] Challenge { get; } = new byte[1024];
        public NetConnection Connection { get; set; }

        public ConnectionStatus ConnectionStatus { get; set; }
        public bool DisconnectClient { get; set; }
        public bool IsBanned { get; set; }
        public long LastReceiveTime { get; set; }
        public long LastSendTime { get; set; }
        public UnityEngine.Color PlayerColor { get; set; }
        public string PlayerName { get; set; } = "Unknown";
        public PlayerStatus PlayerStatus { get; set; }
        public string PublicKey { get; set; }
        public ConcurrentQueue<IServerMessageBase> SendMessageQueue { get; } = new ConcurrentQueue<IServerMessageBase>();
        public int Subspace { get; set; } = -1;
        public float SubspaceRate { get; set; } = 1f;

        public ClientStructure(IPEndPoint endpoint)
        {
            Endpoint = endpoint;
        }

        public override bool Equals(object obj)
        {
            var clientToCompare = obj as ClientStructure;
            return Endpoint.Equals(clientToCompare?.Endpoint);
        }

        public override int GetHashCode()
        {
            return Endpoint?.GetHashCode() ?? 0;
        }
    }
}