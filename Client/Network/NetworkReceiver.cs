using Lidgren.Network;
using LunaClient.Systems;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Chat;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Facility;
using LunaClient.Systems.Flag;
using LunaClient.Systems.Groups;
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
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.Warp;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaCommon.Time;
using System;

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
                while (!NetworkConnection.ResetRequested)
                {
                    if (NetworkMain.ClientConnection.ReadMessage(out var msg))
                    {
                        NetworkStatistics.LastReceiveTime = LunaTime.UtcNow;
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
                                    var deserializedMsg = NetworkMain.SrvMsgFactory.Deserialize(msg.ReadBytes(msg.LengthBytes), LunaTime.UtcNow.Ticks);
                                    if (deserializedMsg != null)
                                    {
                                        EnqueueMessageToSystem(deserializedMsg as IServerMessageBase);
                                    }
                                }
                                catch (Exception e)
                                {
                                    LunaLog.LogError($"[LMP]: Error deserializing message! {e}");
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
                                LunaLog.Log($"[LMP]: LIDGREN: {msg.MessageType} -- {msg.PeekString()}");
                                break;
                        }
                        NetworkMain.ClientConnection.Recycle(msg);
                    }
                    else
                    {
                        LunaDelay.Delay(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
                    }
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
                    SystemsContainer.Get<HandshakeSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Chat:
                    SystemsContainer.Get<ChatSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Settings:
                    SystemsContainer.Get<SettingsSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.PlayerStatus:
                    SystemsContainer.Get<StatusSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.PlayerColor:
                    SystemsContainer.Get<PlayerColorSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.PlayerConnection:
                    SystemsContainer.Get<PlayerConnectionSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Scenario:
                    SystemsContainer.Get<ScenarioSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Kerbal:
                    SystemsContainer.Get<KerbalSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Vessel:
                    switch (((VesselBaseMsgData)msg.Data).VesselMessageType)
                    {
                        case VesselMessageType.Position:
                            SystemsContainer.Get<VesselPositionSystem>().EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Flightstate:
                            SystemsContainer.Get<VesselFlightStateSystem>().EnqueueMessage(msg);
                            break;
                        case VesselMessageType.ListReply:
                        case VesselMessageType.VesselsReply:
                        case VesselMessageType.Proto:
                        case VesselMessageType.ProtoReliable:
                            SystemsContainer.Get<VesselProtoSystem>().EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Dock:
                            SystemsContainer.Get<VesselDockSystem>().EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Remove:
                            SystemsContainer.Get<VesselRemoveSystem>().EnqueueMessage(msg);
                            break;
                    }
                    break;
                case ServerMessageType.CraftLibrary:
                    SystemsContainer.Get<CraftLibrarySystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Flag:
                    SystemsContainer.Get<FlagSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Motd:
                    SystemsContainer.Get<MotdSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Warp:
                    SystemsContainer.Get<WarpSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Admin:
                    SystemsContainer.Get<AdminSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Lock:
                    SystemsContainer.Get<LockSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Mod:
                    SystemsContainer.Get<ModApiSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Groups:
                    SystemsContainer.Get<GroupSystem>().EnqueueMessage(msg);
                    break;
                case ServerMessageType.Facility:
                    SystemsContainer.Get<FacilitySystem>().EnqueueMessage(msg);
                    break;
                default:
                    LunaLog.LogError($"[LMP]: Unhandled Message type {msg.MessageType}");
                    break;
            }
        }
    }
}