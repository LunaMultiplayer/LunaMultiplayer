using System.Collections.Concurrent;
using System.Net;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using Lidgren.Network;

namespace LunaServer.Client
{
    public class ClientStructure
    {
        //vessel tracking
        public string ActiveVessel { get; set; } = "";
        public bool Authenticated { get; set; }
        //Network traffic tracking
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public byte[] Challange { get; set; }
        public NetConnection Connection { get; set; }
        //State tracking
        public ConnectionStatus ConnectionStatus { get; set; }
        public bool DisconnectClient { get; set; }
        //Connection
        public IPEndPoint Endpoint { get; set; }
        public IPAddress IpAddress { get; set; }
        public bool IsBanned { get; set; }
        //Receive split buffer
        public long LastQueueOptimizeTime { get; set; }
        public long LastReceiveTime { get; set; }
        //Send buffer
        public long LastSendTime { get; set; }
        public string PlayerColor { get; set; }
        public string PlayerName { get; set; } = "Unknown";
        public PlayerStatus PlayerStatus { get; set; }
        public string PublicKey { get; set; }
        public ConcurrentQueue<IServerMessageBase> SendMessageQueue { get; } = new ConcurrentQueue<IServerMessageBase>();
        //Subspace tracking
        public int Subspace { get; set; } = -1;
        public float SubspaceRate { get; set; } = 1f;
    }
}