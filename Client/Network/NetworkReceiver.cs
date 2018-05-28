using Lidgren.Network;
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
using LunaClient.Systems.Screenshot;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareAchievements;
using LunaClient.Systems.ShareContracts;
using LunaClient.Systems.ShareFunds;
using LunaClient.Systems.SharePurchaseParts;
using LunaClient.Systems.ShareReputation;
using LunaClient.Systems.ShareScience;
using LunaClient.Systems.ShareScienceSubject;
using LunaClient.Systems.ShareStrategy;
using LunaClient.Systems.ShareTechnology;
using LunaClient.Systems.ShareUpgradeableFacilities;
using LunaClient.Systems.Status;
using LunaClient.Systems.VesselDockSys;
using LunaClient.Systems.VesselEvaSys;
using LunaClient.Systems.VesselFairingsSys;
using LunaClient.Systems.VesselFlightStateSys;
using LunaClient.Systems.VesselPartModuleSyncSys;
using LunaClient.Systems.VesselPositionSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselResourceSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.Warp;
using LunaCommon.Enums;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using LunaCommon.Time;
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
                while (!NetworkConnection.ResetRequested)
                {
                    if (NetworkMain.ClientConnection.ReadMessage(out var msg))
                    {
                        NetworkStatistics.LastReceiveTime = LunaNetworkTime.UtcNow;
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.DebugMessage:
                                LunaLog.Log("[Lidgen DEBUG] " + msg.ReadString());
                                break;
                            case NetIncomingMessageType.VerboseDebugMessage:
                                LunaLog.Log("[Lidgen VERBOSE] " + msg.ReadString());
                                break;
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
                                    var deserializedMsg = NetworkMain.SrvMsgFactory.Deserialize(msg, LunaNetworkTime.UtcNow.Ticks);
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
                        Thread.Sleep(SettingsSystem.CurrentSettings.SendReceiveMsInterval);
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
                case ServerMessageType.Admin:
                    AdminSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Handshake:
                    HandshakeSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Chat:
                    ChatSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Settings:
                    SettingsSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.PlayerStatus:
                    StatusSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.PlayerColor:
                    PlayerColorSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.PlayerConnection:
                    PlayerConnectionSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Scenario:
                    ScenarioSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Kerbal:
                    KerbalSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Vessel:
                    switch (((VesselBaseMsgData)msg.Data).VesselMessageType)
                    {
                        case VesselMessageType.Position:
                            VesselPositionSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Flightstate:
                            VesselFlightStateSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Proto:
                            VesselProtoSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Dock:
                            VesselDockSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Remove:
                            VesselRemoveSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Update:
                            VesselUpdateSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Resource:
                            VesselResourceSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.PartSync:
                            VesselPartModuleSyncSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Fairing:
                            VesselFairingsSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case VesselMessageType.Eva:
                            VesselEvaSystem.Singleton.EnqueueMessage(msg);
                            break;
                    }
                    break;
                case ServerMessageType.CraftLibrary:
                    CraftLibrarySystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Flag:
                    FlagSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Motd:
                    MotdSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Warp:
                    WarpSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Lock:
                    LockSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Mod:
                    ModApiSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Groups:
                    GroupSystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.Facility:
                    FacilitySystem.Singleton.EnqueueMessage(msg);
                    break;
                case ServerMessageType.ShareProgress:
                    switch (((ShareProgressBaseMsgData)msg.Data).ShareProgressMessageType)
                    {
                        case ShareProgressMessageType.FundsUpdate:
                            ShareFundsSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.ScienceUpdate:
                            ShareScienceSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.ScienceSubjectUpdate:
                            ShareScienceSubjectSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.ReputationUpdate:
                            ShareReputationSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.TechnologyUpdate:
                            ShareTechnologySystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.ContractsUpdate:
                            ShareContractsSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.AchievementsUpdate:
                            ShareAchievementsSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.StrategyUpdate:
                            ShareStrategySystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.FacilityUpgrade:
                            ShareUpgradeableFacilitiesSystem.Singleton.EnqueueMessage(msg);
                            break;
                        case ShareProgressMessageType.PartPurchase:
                            SharePurchasePartsSystem.Singleton.EnqueueMessage(msg);
                            break;
                    }
                    break;
                case ServerMessageType.Screenshot:
                    ScreenshotSystem.Singleton.EnqueueMessage(msg);
                    break;
                default:
                    LunaLog.LogError($"[LMP]: Unhandled Message type {msg.MessageType}");
                    break;
            }
        }
    }
}
