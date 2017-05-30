using Lidgren.Network;
using LunaClient.Systems;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Chat;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Flag;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.ModApi;
using LunaClient.Systems.Motd;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselChangeSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Threading;

namespace LunaClient.Network
{
    public class NetworkReceiver
    {
        /// <summary>
        /// Main receiveing thread
        /// </summary>
        public static void ReceiveMain()
        {
            try
            {
                while (!SystemsContainer.Get<MainSystem>().Quit)
                {
                    while (NetworkMain.ClientConnection.ReadMessage(out var msg))
                    {
                        NetworkStatistics.LastReceiveTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.NatIntroductionSuccess:
                                NetworkServerList.HandleNatIntroduction(msg);
                                break;
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                                NetworkStatistics.PingMs = (float)TimeSpan.FromSeconds(msg.ReadFloat()).TotalMilliseconds;
                                break;
                            case NetIncomingMessageType.UnconnectedData:
                                NetworkServerList.HandleServersList(msg);
                                break;
                            case NetIncomingMessageType.Data:
                                try
                                {
                                    var deserializedMsg = NetworkMain.SrvMsgFactory.Deserialize(msg.ReadBytes(msg.LengthBytes), DateTime.UtcNow.Ticks);
                                    if (deserializedMsg != null)
                                    {
                                        EnqueueMessageToSystem(deserializedMsg as IServerMessageBase);
                                    }
                                }
                                catch (Exception e)
                                {
                                    LunaLog.LogError($"[LMP]: Error deserializing message! {e}");
                                    NetworkMain.HandleDisconnectException(e);
                                }
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                switch ((NetConnectionStatus)msg.ReadByte())
                                {
                                    case NetConnectionStatus.Disconnected:
                                        var reason = msg.ReadString();
                                        NetworkConnection.Disconnect(reason);
                                        break;
                                }
                                break;
                            default:
                                LunaLog.Log($"[LMP]: LIDGREN: {msg.MessageType}-- {msg.PeekString()}");
                                break;
                        }
                    }

                    Thread.Sleep(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Receive message thread error: {e}");
                NetworkMain.HandleDisconnectException(e);
            }
        }

        /// <summary>
        /// Enqueues the received message to the correct system
        /// </summary>
        /// <param name="msg"></param>
        private static void EnqueueMessageToSystem(IServerMessageBase msg)
        {
            switch (msg.MessageType)
            {
                case ServerMessageType.Handshake:
                    SystemsContainer.Get<HandshakeSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Chat:
                    SystemsContainer.Get<ChatSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Settings:
                    SystemsContainer.Get<SettingsSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.PlayerStatus:
                    SystemsContainer.Get<StatusSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.PlayerColor:
                    SystemsContainer.Get<PlayerColorSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.PlayerConnection:
                    SystemsContainer.Get<PlayerConnectionSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Scenario:
                    SystemsContainer.Get<ScenarioSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Kerbal:
                    SystemsContainer.Get<KerbalSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Vessel:
                    switch (((VesselBaseMsgData)msg.Data).VesselMessageType)
                    {
                        case VesselMessageType.Update:
                            SystemsContainer.Get<VesselUpdateSystem>().EnqueueMessage(msg.Data);
                            break;
                        case VesselMessageType.Position:
                            SystemsContainer.Get<VesselPositionSystem>().EnqueueMessage(msg.Data);
                            break;
                        case VesselMessageType.Flightstate:
                            SystemsContainer.Get<VesselFlightStateSystem>().EnqueueMessage(msg.Data);
                            break;
                        case VesselMessageType.Change:
                            SystemsContainer.Get<VesselChangeSystem>().EnqueueMessage(msg.Data);
                            break;
                        case VesselMessageType.ListReply:
                        case VesselMessageType.VesselsReply:
                        case VesselMessageType.Proto:
                            SystemsContainer.Get<VesselProtoSystem>().EnqueueMessage(msg.Data);
                            break;
                        case VesselMessageType.Remove:
                            SystemsContainer.Get<VesselRemoveSystem>().EnqueueMessage(msg.Data);
                            break;
                    }
                    break;
                case ServerMessageType.CraftLibrary:
                    SystemsContainer.Get<CraftLibrarySystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Flag:
                    SystemsContainer.Get<FlagSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.SyncTime:
                    SystemsContainer.Get<TimeSyncerSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Motd:
                    SystemsContainer.Get<MotdSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Warp:
                    SystemsContainer.Get<WarpSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Admin:
                    SystemsContainer.Get<AdminSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Lock:
                    SystemsContainer.Get<LockSystem>().EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.Mod:
                    SystemsContainer.Get<ModApiSystem>().EnqueueMessage(msg.Data);
                    break;
                default:
                    LunaLog.LogError($"[LMP]: Unhandled Message type {msg.MessageType}");
                    break;
            }
        }
    }
}