using Lidgren.Network;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using Server.Context;
using Server.Plugin;
using Server.Settings;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;

namespace Server.Client
{
    public class ClientStructure
    {
        public IPEndPoint Endpoint => Connection.RemoteEndPoint;
        public Guid Id { get; set; }

        public string ActiveVessel { get; set; } = "";
        public bool Authenticated { get; set; }

        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public byte[] Challenge { get; } = new byte[1024];
        public NetConnection Connection { get; }

        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Connected;
        public bool DisconnectClient { get; set; }
        public bool IsBanned { get; set; }
        public long LastReceiveTime { get; set; } = ServerContext.ServerClock.ElapsedMilliseconds;
        public long LastSendTime { get; set; } = 0;
        public UnityEngine.Color PlayerColor { get; set; }
        public string PlayerName { get; set; } = "Unknown";
        public PlayerStatus PlayerStatus { get; set; } = new PlayerStatus();
        public string PublicKey { get; set; }
        public ConcurrentQueue<IServerMessageBase> SendMessageQueue { get; } = new ConcurrentQueue<IServerMessageBase>();
        public int Subspace { get; set; } = int.MinValue;
        public float SubspaceRate { get; set; } = 1f;

        public Task SendThread { get; }

        public ClientStructure(NetConnection playerConnection)
        {
            Connection = playerConnection;

            SendThread = new Task(SendMessagesThread);
            SendThread.Start();
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

        private async void SendMessagesThread()
        {
            while (ConnectionStatus == ConnectionStatus.Connected)
            {
                if (SendMessageQueue.TryDequeue(out var message) && message != null)
                {
                    try
                    {
                        ServerContext.LidgrenServer.SendMessageToClient(this, message);
                    }
                    catch (Exception e)
                    {
                        ClientException.HandleDisconnectException("Send network message error: ", this, e);
                        return;
                    }

                    LmpPluginHandler.FireOnMessageSent(this, message);
                }
                else
                {
                    await Task.Delay(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
                }
            }
        }
    }
}