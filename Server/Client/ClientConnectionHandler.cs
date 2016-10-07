using System;
using System.Threading.Tasks;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Server;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Plugin;
using LunaServer.Server;
using LunaServer.Settings;
using LunaServer.System;
using Lidgren.Network;

namespace LunaServer.Client
{
    public class ClientConnectionHandler
    {
        public static void ConnectClient(NetConnection newClientConnection)
        {
            var newClientObject = new ClientStructure
            {
                Subspace = 0,
                PlayerStatus = new PlayerStatus(),
                ConnectionStatus = ConnectionStatus.CONNECTED,
                Endpoint = newClientConnection.RemoteEndPoint,
                IpAddress = newClientConnection.RemoteEndPoint.Address,
                Connection = newClientConnection,
                LastSendTime = 0,
                LastReceiveTime = ServerContext.ServerClock.ElapsedMilliseconds
            };

            Task.Run(() => MessageSender.StartSendingOutgoingMessages(newClientObject));

            LmpPluginHandler.FireOnClientConnect(newClientObject);
            
            ServerContext.Clients.TryAdd(newClientObject.Endpoint, newClientObject);
            VesselUpdateRelay.AddPlayer(newClientObject);
            LunaLog.Debug("Online Players: " + ServerContext.PlayerCount + ", connected: " + ServerContext.Clients.Count);
        }

        public static void DisconnectClient(ClientStructure client, string reason ="")
        {
            if (!string.IsNullOrEmpty(reason))
                LunaLog.Debug($"{client.PlayerName} sent Connection end message, reason: {reason}");

            VesselUpdateRelay.RemovePlayer(client);

            //Remove Clients from list
            if (ServerContext.Clients.ContainsKey(client.Endpoint))
            {
                ServerContext.Clients.TryRemove(client.Endpoint, out client);
                LunaLog.Debug($"Online Players: {ServerContext.PlayerCount}, connected: {ServerContext.Clients.Count}");

                WarpSystem.DisconnectPlayer(client.PlayerName);
            }

            if (client.ConnectionStatus != ConnectionStatus.DISCONNECTED)
            {
                client.ConnectionStatus = ConnectionStatus.DISCONNECTED;
                LmpPluginHandler.FireOnClientDisconnect(client);
                if (client.Authenticated)
                {
                    ChatSystem.RemovePlayer(client.PlayerName);
                    MessageQueuer.RelayMessage<PlayerConnectionSrvMsg>(client, new PlayerConnectionLeaveMsgData { PlayerName = client.PlayerName });
                    LockSystem.ReleasePlayerLocks(client.PlayerName);
                }

                try
                {
                    client.Connection?.Disconnect(reason);
                }
                catch (Exception e)
                {
                    LunaLog.Debug("Error closing client Connection: " + e.Message);
                }
                ServerContext.LastPlayerActivity = ServerContext.ServerClock.ElapsedMilliseconds;
            }
        }
    }
}