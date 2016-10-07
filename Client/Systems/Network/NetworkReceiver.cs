using System;
using LunaClient.Systems.Admin;
using LunaClient.Systems.Chat;
using LunaClient.Systems.ColorSystem;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.Flag;
using LunaClient.Systems.Handshake;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.ModApi;
using LunaClient.Systems.Motd;
using LunaClient.Systems.PlayerConnection;
using LunaClient.Systems.Scenario;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;
using UnityEngine;

namespace LunaClient.Systems.Network
{
    public partial class NetworkSystem
    {
        private void ReceiveThreadMain()
        {
            try
            {
                while (ClientConnection != null && MainSystem.Singleton.NetworkState >= ClientState.CONNECTED)
                {
                    NetIncomingMessage msg;
                    if (ClientConnection.ReadMessage(out msg))
                    {
                        LastReceiveTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                                PingMs = TimeSpan.FromSeconds(msg.ReadFloat()).TotalMilliseconds;
                                break;
                            case NetIncomingMessageType.Data:
                                try
                                {
                                    var deserializedMsg = ServerMessageFactory.Deserialize(msg.ReadBytes(msg.LengthBytes), DateTime.UtcNow.Ticks);
                                    EnqueueMessageToSystem(deserializedMsg as IServerMessageBase);
                                }
                                catch (Exception e)
                                {
                                    LunaLog.Debug("Error deserializing message!");
                                    HandleDisconnectException(e);
                                }
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                switch ((NetConnectionStatus)msg.ReadByte())
                                {
                                    case NetConnectionStatus.Disconnected:
                                        var reason = msg.ReadString();
                                        Disconnect(reason);
                                        break;
                                }
                                break;
                            default:
                                LunaLog.Debug("LIDGREN: " + msg.MessageType + "-- " + msg.PeekString());
                                break;
                        }
                    }
                    else
                    {
                        MainSystem.Delay(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.Debug("Receive message thread error: " + e);
                HandleDisconnectException(e);
            }
        }

        private void EnqueueMessageToSystem(IServerMessageBase msg)
        {
#if DEBUG
            switch (msg.MessageType)
            {
                case ServerMessageType.VESSEL:
                    switch (((VesselBaseMsgData)msg.Data).VesselMessageType)
                    {
                        //case VesselMessageType.UPDATE:
                        case VesselMessageType.LIST_REPLY:
                        case VesselMessageType.VESSELS_REPLY:
                        case VesselMessageType.PROTO:
                        case VesselMessageType.REMOVE:
                            Debug.Log($"Received {msg.MessageType}-{msg.Data.GetType().Name} from the srv.");
                            break;
                    }
                    break;
                case ServerMessageType.HANDSHAKE:
                case ServerMessageType.CHAT:
                case ServerMessageType.SETTINGS:
                case ServerMessageType.PLAYER_STATUS:
                case ServerMessageType.PLAYER_COLOR:
                case ServerMessageType.PLAYER_CONNECTION:
                case ServerMessageType.SCENARIO:
                case ServerMessageType.KERBAL:
                case ServerMessageType.CRAFT_LIBRARY:
                case ServerMessageType.FLAG:
                //case ServerMessageType.SYNC_TIME:
                case ServerMessageType.MOTD:
                //case ServerMessageType.WARP:
                case ServerMessageType.ADMIN:
                case ServerMessageType.LOCK:
                case ServerMessageType.MOD:
                    Debug.Log($"Received {msg.MessageType}-{msg.Data.GetType().Name} from the srv.");
                    break;
            }
#endif

            switch (msg.MessageType)
            {
                case ServerMessageType.HANDSHAKE:
                    HandshakeSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.CHAT:
                    ChatSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.SETTINGS:
                    SettingsSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.PLAYER_STATUS:
                    StatusSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.PLAYER_COLOR:
                    PlayerColorSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.PLAYER_CONNECTION:
                    PlayerConnectionSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.SCENARIO:
                    ScenarioSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.KERBAL:
                    KerbalSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.VESSEL:
                    switch (((VesselBaseMsgData)msg.Data).VesselMessageType)
                    {
                        case VesselMessageType.UPDATE:
                            VesselUpdateSystem.Singleton.EnqueueMessage(msg.Data);
                            break;
                        case VesselMessageType.LIST_REPLY:
                        case VesselMessageType.VESSELS_REPLY:
                        case VesselMessageType.PROTO:
                            VesselProtoSystem.Singleton.EnqueueMessage(msg.Data);
                            break;
                        case VesselMessageType.REMOVE:
                            VesselRemoveSystem.Singleton.EnqueueMessage(msg.Data);
                            break;
                    }
                    break;
                case ServerMessageType.CRAFT_LIBRARY:
                    CraftLibrarySystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.FLAG:
                    FlagSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.SYNC_TIME:
                    TimeSyncerSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.MOTD:
                    MotdSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.WARP:
                    WarpSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.ADMIN:
                    AdminSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.LOCK:
                    LockSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                case ServerMessageType.MOD:
                    ModApiSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                default:
                    LunaLog.Debug("Unhandled Message type " + msg.MessageType);
                    break;
            }
        }
    }
}