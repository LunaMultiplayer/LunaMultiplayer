using Lidgren.Network;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer;
using LunaCommon.Time;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings;
using Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Server.Lidgren
{
    public class LidgrenServer
    {
        private static List<IPEndPoint> MasterServerEndpoints { get; } = new List<IPEndPoint>();
        private static NetServer Server { get; set; }
        public static MessageReceiver ClientMessageReceiver { get; set; } = new MessageReceiver();

        private static int MasterServerRegistrationMsInterval => GeneralSettings.SettingsStore.MasterServerRegistrationMsInterval < 5000 ? 
            5000 : GeneralSettings.SettingsStore.MasterServerRegistrationMsInterval;

        public void SetupLidgrenServer()
        {
            if (Common.PortIsInUse(ServerContext.Config.Port))
            {
                throw new HandledException($"Port {ServerContext.Config.Port} is already in use");
            }

            ServerContext.Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            ServerContext.Config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);

#if DEBUG
            ServerContext.Config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            //ServerContext.Config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            if (DebugSettings.SettingsStore?.SimulatedLossChance < 100 && DebugSettings.SettingsStore?.SimulatedLossChance > 0)
            {
                ServerContext.Config.SimulatedLoss = DebugSettings.SettingsStore.SimulatedLossChance / 100f;
            }
            if (DebugSettings.SettingsStore?.SimulatedDuplicatesChance < 100 && DebugSettings.SettingsStore?.SimulatedLossChance > 0)
            {
                ServerContext.Config.SimulatedDuplicatesChance = DebugSettings.SettingsStore.SimulatedDuplicatesChance / 100f;
            }

            ServerContext.Config.SimulatedRandomLatency = (float)TimeSpan.FromMilliseconds(DebugSettings.SettingsStore?.MaxSimulatedRandomLatencyMs ?? 0).TotalSeconds;
            ServerContext.Config.SimulatedMinimumLatency = (float)TimeSpan.FromMilliseconds(DebugSettings.SettingsStore?.MinSimulatedLatencyMs ?? 0).TotalSeconds;
#endif

            Server = new NetServer(ServerContext.Config);
            Server.Start();
            ServerContext.ServerStarting = false;
        }

        public async void StartReceiveingMessages()
        {
            try
            {
                while (ServerContext.ServerRunning)
                {
                    var msg = Server.ReadMessage();
                    if (msg != null)
                    {
                        var client = TryGetClient(msg);
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionApproval:
                                msg.SenderConnection.Approve();
                                break;
                            case NetIncomingMessageType.Data:
                                ClientMessageReceiver.ReceiveCallback(client, msg);
                                break;
                            case NetIncomingMessageType.WarningMessage:
                                LunaLog.Warning(msg.ReadString());
                                break;
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.VerboseDebugMessage:
                                LunaLog.NetworkDebug(msg.ReadString());
                                break;
                            case NetIncomingMessageType.Error:
                                LunaLog.Error(msg.ReadString());
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                switch ((NetConnectionStatus)msg.ReadByte())
                                {
                                    case NetConnectionStatus.Connected:
                                        var endpoint = msg.SenderConnection.RemoteEndPoint;
                                        LunaLog.Normal($"New client Connection from {endpoint.Address}:{endpoint.Port}");
                                        ClientConnectionHandler.ConnectClient(msg.SenderConnection);
                                        break;
                                    case NetConnectionStatus.Disconnected:
                                        var reason = msg.ReadString();
                                        if (client != null)
                                            ClientConnectionHandler.DisconnectClient(client, reason);
                                        break;
                                }
                                break;
                            default:
                                var details = msg.PeekString();
                                LunaLog.Debug($"Lidgren: {msg.MessageType.ToString().ToUpper()} -- {details}");
                                break;
                        }
                    }
                    else
                    {
                        await Task.Delay(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.Fatal($"ERROR in thread receive! Details: {e}");
            }
        }

        private static ClientStructure TryGetClient(NetIncomingMessage msg)
        {
            if (msg.SenderConnection != null)
            {
                ServerContext.Clients.TryGetValue(msg.SenderConnection.RemoteEndPoint, out var client);
                return client;
            }
            return null;
        }

        public void SendMessageToClient(ClientStructure client, IServerMessageBase message)
        {
            var outmsg = Server.CreateMessage(message.GetMessageSize());

            message.Data.SentTime = LunaTime.UtcNow.Ticks;
            message.Serialize(outmsg);

            client.LastSendTime = ServerContext.ServerClock.ElapsedMilliseconds;
            client.BytesSent += outmsg.LengthBytes;
            
            var sendResult = Server.SendMessage(outmsg, client.Connection, message.NetDeliveryMethod, message.Channel);
            Server.FlushSendQueue(); //Manually force to send the msg
        }

        public void ShutdownLidgrenServer()
        {
            Server.Shutdown("Goodbye and thanks for all the fish");
        }

        public async void RefreshMasterServersList()
        {
            if (!GeneralSettings.SettingsStore.RegisterWithMasterServer) return;

            while (ServerContext.ServerRunning)
            {
                lock (MasterServerEndpoints)
                {
                    MasterServerEndpoints.Clear();
                    MasterServerEndpoints.AddRange(MasterServerRetriever.RetrieveWorkingMasterServersEndpoints()
                        .Select(Common.CreateEndpointFromString));
                }

                await Task.Delay((int)TimeSpan.FromMinutes(10).TotalMilliseconds);
            }
        }

        public async void RegisterWithMasterServer()
        {
            if (!GeneralSettings.SettingsStore.RegisterWithMasterServer) return;

            LunaLog.Normal("Registering with master servers...");

            var adr = LunaNetUtils.GetMyAddress();
            if (adr == null) return;

            var endpoint = new IPEndPoint(adr, ServerContext.Config.Port);
            while (ServerContext.ServerRunning)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<MsRegisterServerMsgData>();
                msgData.Id = Server.UniqueIdentifier;
                msgData.Cheats = GeneralSettings.SettingsStore.Cheats;
                msgData.ShowVesselsInThePast = GeneralSettings.SettingsStore.ShowVesselsInThePast;
                msgData.Description = GeneralSettings.SettingsStore.Description;
                msgData.DropControlOnExit = GeneralSettings.SettingsStore.Cheats;
                msgData.DropControlOnExitFlight = GeneralSettings.SettingsStore.Cheats;
                msgData.DropControlOnVesselSwitching = GeneralSettings.SettingsStore.Cheats;
                msgData.GameMode = (int)GeneralSettings.SettingsStore.GameMode;
                msgData.InternalEndpoint = $"{endpoint.Address}:{endpoint.Port}";
                msgData.MaxPlayers = GeneralSettings.SettingsStore.MaxPlayers;
                msgData.ModControl = (int)GeneralSettings.SettingsStore.ModControl;
                msgData.PlayerCount = ServerContext.Clients.Count;
                msgData.ServerName = GeneralSettings.SettingsStore.ServerName;
                msgData.ServerVersion = LmpVersioning.CurrentVersion;
                msgData.VesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.VesselUpdatesSendMsInterval;
                msgData.SecondaryVesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.SecondaryVesselUpdatesSendMsInterval;
                msgData.WarpMode = (int)GeneralSettings.SettingsStore.WarpMode;
                msgData.TerrainQuality = (int)GeneralSettings.SettingsStore.TerrainQuality;

                msgData.Description = msgData.Description.Length > 200
                            ? msgData.Description.Substring(0, 200)
                            : msgData.Description;

                msgData.ServerName = msgData.ServerName.Length > 30
                    ? msgData.ServerName.Substring(0, 30)
                    : msgData.ServerName;

                lock (MasterServerEndpoints)
                {
                    foreach (var masterServer in MasterServerEndpoints)
                    {
                        RegisterWithMasterServer(msgData, masterServer);
                    }
                }

                await Task.Delay(MasterServerRegistrationMsInterval);
            }
        }

        private static void RegisterWithMasterServer(MsRegisterServerMsgData msgData, IPEndPoint masterServer)
        {
            Task.Run(() =>
            {
                var msg = ServerContext.MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
                msg.Data.SentTime = LunaTime.UtcNow.Ticks;
                
                try
                {
                    var outMsg = Server.CreateMessage(msg.GetMessageSize());
                    msg.Serialize(outMsg);
                    Server.SendUnconnectedMessage(outMsg, masterServer);
                    Server.FlushSendQueue();
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }
    }
}