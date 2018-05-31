using Lidgren.Network;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Interface;
using Server.Context;
using Server.Lidgren;
using Server.Plugin;
using Server.Settings.Structures;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Client
{
    public class ClientStructure
    {
        public IPEndPoint Endpoint => Connection.RemoteEndPoint;

        public string UniqueIdentifier { get; set; }
        
        public bool Authenticated { get; set; }

        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public NetConnection Connection { get; }

        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Connected;
        public bool DisconnectClient { get; set; }
        public long LastReceiveTime { get; set; } = ServerContext.ServerClock.ElapsedMilliseconds;
        public long LastSendTime { get; set; } = 0;
        public UnityEngine.Color PlayerColor { get; set; }
        public string PlayerName { get; set; } = "Unknown";
        public PlayerStatus PlayerStatus { get; set; } = new PlayerStatus();
        public ConcurrentQueue<IServerMessageBase> SendMessageQueue { get; } = new ConcurrentQueue<IServerMessageBase>();
        public int Subspace { get; set; } = WarpContext.LatestSubspace;
        public float SubspaceRate { get; set; } = 1f;

        public Task SendThread { get; }

        public ClientStructure(NetConnection playerConnection)
        {
            Connection = playerConnection;
            SendThread = MainServer.LongRunTaskFactory.StartNew(() => SendMessagesThread(MainServer.CancellationTokenSrc.Token), MainServer.CancellationTokenSrc.Token);
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

        private async void SendMessagesThread(CancellationToken token)
        {
            while (ConnectionStatus == ConnectionStatus.Connected)
            {
                while(SendMessageQueue.TryDequeue(out var message) && message != null)
                {
                    try
                    {
                        LidgrenServer.SendMessageToClient(this, message);
                    }
                    catch (Exception e)
                    {
                        ClientException.HandleDisconnectException("Send network message error: ", this, e);
                        return;
                    }

                    LmpPluginHandler.FireOnMessageSent(this, message);
                }
                try
                {
                    await Task.Delay(IntervalSettings.SettingsStore.SendReceiveThreadTickMs, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
