using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Message.Reader.Base;
using Server.Server;
using Server.Settings;
using Server.System;
using Server.System.VesselRelay;
using System;
using System.Linq;
using System.Text;

namespace Server.Message.Reader
{
    public class VesselMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as VesselBaseMsgData;
            switch (message?.VesselMessageType)
            {
                case VesselMessageType.Sync:
                    HandleVesselsSync(client, message);
                    break;
                case VesselMessageType.Proto:
                    HandleVesselProto(client, message);
                    break;
                case VesselMessageType.Dock:
                    HandleVesselDock(client, message);
                    break;
                case VesselMessageType.Remove:
                    HandleVesselRemove(client, message);
                    break;
                case VesselMessageType.Position:
                    VesselRelaySystem.HandleVesselMessage(client, message);
                    if (!GeneralSettings.SettingsStore.ShowVesselsInThePast || client.Subspace == WarpContext.LatestSubspace)
                        VesselDataUpdater.WritePositionDataToFile(message);
                    break;
                case VesselMessageType.Flightstate:
                    VesselRelaySystem.HandleVesselMessage(client, message);
                    break;
                case VesselMessageType.Update:
                    VesselDataUpdater.WriteUpdateDataToFile(message);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, message);
                    break;
                case VesselMessageType.Resource:
                    VesselDataUpdater.WriteResourceDataToFile(message);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, message);
                    break;
                case VesselMessageType.PartSync:
                    VesselDataUpdater.WriteModuleDataToFile(message);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, message);
                    break;
                default:
                    throw new NotImplementedException("Vessel message type not implemented");
            }
        }

        private static void HandleVesselRemove(ClientStructure client, VesselBaseMsgData message)
        {
            var data = (VesselRemoveMsgData) message;

            if (LockSystem.LockQuery.ControlLockExists(data.VesselId) && !LockSystem.LockQuery.ControlLockBelongsToPlayer(data.VesselId, client.PlayerName))
                return;

            if (VesselStoreSystem.VesselExists(data.VesselId))
            {
                LunaLog.Debug($"Removing vessel {data.VesselId} from {client.PlayerName}");
                VesselStoreSystem.RemoveVessel(data.VesselId);
            }

            if (data.AddToKillList)
                VesselContext.RemovedVessels.Add(data.VesselId);

            //Relay the message.
            MessageQueuer.SendToAllClients<VesselSrvMsg>(data);
        }

        private static void HandleVesselProto(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselProtoMsgData) message;

            if (VesselContext.RemovedVessels.Contains(msgData.Vessel.VesselId)) return;

            if (msgData.Vessel.NumBytes == 0)
            {
                LunaLog.Warning($"Received a vessel with 0 bytes ({msgData.Vessel.VesselId}) from {client.PlayerName}.");
                return;
            }

            if (!VesselStoreSystem.VesselExists(msgData.Vessel.VesselId))
            {
                LunaLog.Debug($"Saving vessel {msgData.Vessel.VesselId} from {client.PlayerName}. Bytes: {msgData.Vessel.NumBytes}");
            }

            VesselDataUpdater.RawConfigNodeInsertOrUpdate(msgData.Vessel.VesselId, Encoding.UTF8.GetString(msgData.Vessel.Data, 0, msgData.Vessel.NumBytes));
            MessageQueuer.RelayMessage<VesselSrvMsg>(client, msgData);
        }

        private static void HandleVesselDock(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselDockMsgData) message;

            LunaLog.Debug($"Docking message received! Dominant vessel: {msgData.DominantVesselId}");

            if (VesselContext.RemovedVessels.Contains(msgData.WeakVesselId)) return;

            if (VesselStoreSystem.VesselExists(msgData.DominantVesselId))
            {
                LunaLog.Debug($"Saving DOCKED vessel {msgData.DominantVesselId} from {client.PlayerName}. Bytes: {msgData.NumBytes}");
            }
            VesselDataUpdater.RawConfigNodeInsertOrUpdate(msgData.DominantVesselId, Encoding.UTF8.GetString(msgData.FinalVesselData, 0, msgData.NumBytes));

            //Now remove the weak vessel but DO NOT add to the removed vessels as they might undock!!!
            LunaLog.Debug($"Removing weak docked vessel {msgData.WeakVesselId}");
            VesselStoreSystem.RemoveVessel(msgData.WeakVesselId);

            MessageQueuer.RelayMessage<VesselSrvMsg>(client, msgData);

            //Tell all clients to remove the weak vessel
            var removeMsgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
            removeMsgData.VesselId = msgData.WeakVesselId;

            MessageQueuer.SendToAllClients<VesselSrvMsg>(removeMsgData);
        }

        private static void HandleVesselsSync(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselSyncMsgData) message;

            var vesselsToSend = VesselStoreSystem.CurrentVesselsInXmlFormat.Keys.Except(msgData.VesselIds).ToArray();
            foreach (var vesselId in vesselsToSend)
            {
                var vesselData = VesselStoreSystem.GetVesselInConfigNodeFormat(vesselId);
                if (vesselData.Length > 0)
                {
                    var protoMsg = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselProtoMsgData>();
                    protoMsg.Vessel.Data = Encoding.UTF8.GetBytes(vesselData);
                    protoMsg.Vessel.NumBytes = vesselData.Length;
                    protoMsg.Vessel.VesselId = vesselId;

                    MessageQueuer.SendToClient<VesselSrvMsg>(client, protoMsg);
                }
            }

            if (vesselsToSend.Length > 0)
                LunaLog.Debug($"Sending {client.PlayerName} {vesselsToSend.Length} vessels");
        }
    }
}
