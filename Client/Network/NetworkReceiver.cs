using Lidgren.Network;
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
using UnityEngine;

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
                while (!MainSystem.Singleton.Quit)
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
                                Debug.LogError("[LMP]: Error deserializing message! {e}");
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
                            Debug.Log("[LMP]: LIDGREN: " + msg.MessageType + "-- " + msg.PeekString());
                            break;
                        }
                    }

                    Thread.Sleep(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[LMP]: Receive message thread error: " + e);
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
                HandshakeSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Chat:
                ChatSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Settings:
                SettingsSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.PlayerStatus:
                StatusSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.PlayerColor:
                PlayerColorSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.PlayerConnection:
                PlayerConnectionSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Scenario:
                ScenarioSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Kerbal:
                KerbalSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Vessel:
                switch (((VesselBaseMsgData)msg.Data).VesselMessageType)
                {
                    case VesselMessageType.Update:
                    VesselUpdateSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                    case VesselMessageType.Position:
                    VesselPositionSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                    case VesselMessageType.Flightstate:
                    VesselFlightStateSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                    case VesselMessageType.Change:
                    VesselChangeSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                    case VesselMessageType.ListReply:
                    case VesselMessageType.VesselsReply:
                    case VesselMessageType.Proto:
                    VesselProtoSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                    case VesselMessageType.Remove:
                    VesselRemoveSystem.Singleton.EnqueueMessage(msg.Data);
                    break;
                }
                break;
                case ServerMessageType.CraftLibrary:
                CraftLibrarySystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Flag:
                FlagSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.SyncTime:
                TimeSyncerSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Motd:
                MotdSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Warp:
                WarpSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Admin:
                AdminSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Lock:
                LockSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                case ServerMessageType.Mod:
                ModApiSystem.Singleton.EnqueueMessage(msg.Data);
                break;
                default:
                Debug.LogError("[LMP]: Unhandled Message type " + msg.MessageType);
                break;
            }
        }
    }
}