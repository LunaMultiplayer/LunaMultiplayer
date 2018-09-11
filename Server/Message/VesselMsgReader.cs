using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Message.Base;
using Server.Server;
using Server.System;
using System;
using System.Linq;
using System.Text;

namespace Server.Message
{
    public class VesselMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as VesselBaseMsgData;
            switch (messageData?.VesselMessageType)
            {
                case VesselMessageType.Sync:
                    HandleVesselsSync(client, messageData);
                    message.Recycle();
                    break;
                case VesselMessageType.Proto:
                    HandleVesselProto(client, messageData);
                    break;
                case VesselMessageType.Dock:
                    HandleVesselDock(client, messageData);
                    break;
                case VesselMessageType.Remove:
                    HandleVesselRemove(client, messageData);
                    break;
                case VesselMessageType.Position:
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, messageData);
                    if (client.Subspace == WarpContext.LatestSubspace.Id)
                        VesselDataUpdater.WritePositionDataToFile(messageData);
                    break;
                case VesselMessageType.Flightstate:
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, messageData);
                    VesselDataUpdater.WriteFlightstateDataToFile(messageData);
                    break;
                case VesselMessageType.Update:
                    VesselDataUpdater.WriteUpdateDataToFile(messageData);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, messageData);
                    break;
                case VesselMessageType.Resource:
                    VesselDataUpdater.WriteResourceDataToFile(messageData);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, messageData);
                    break;
                case VesselMessageType.PartSync:
                    VesselDataUpdater.WritePartSyncDataToFile(messageData);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, messageData);
                    break;
                case VesselMessageType.Fairing:
                    VesselDataUpdater.WriteFairingDataToFile(messageData);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, messageData);
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
            MessageQueuer.RelayMessage<VesselSrvMsg>(client, data);
        }

        private static void HandleVesselProto(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselProtoMsgData) message;

            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            if (msgData.NumBytes == 0)
            {
                LunaLog.Warning($"Received a vessel with 0 bytes ({msgData.VesselId}) from {client.PlayerName}.");
                return;
            }

            if (!VesselStoreSystem.VesselExists(msgData.VesselId))
            {
                LunaLog.Debug($"Saving vessel {msgData.VesselId} from {client.PlayerName}. Bytes: {msgData.NumBytes}");
            }

            VesselDataUpdater.RawConfigNodeInsertOrUpdate(msgData.VesselId, Encoding.UTF8.GetString(msgData.Data, 0, msgData.NumBytes));
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

            var allVessels = VesselStoreSystem.CurrentVesselsInXmlFormat.Keys.ToList();
            for (var i = 0; i < msgData.VesselsCount; i++)
            {
                allVessels.Remove(msgData.VesselIds[i]);
            }

            var vesselsToSend = allVessels;
            foreach (var vesselId in vesselsToSend)
            {
                var vesselData = VesselStoreSystem.GetVesselInConfigNodeFormat(vesselId);
                if (vesselData.Length > 0)
                {
                    var protoMsg = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselProtoMsgData>();
                    protoMsg.Data = Encoding.UTF8.GetBytes(vesselData);
                    protoMsg.NumBytes = vesselData.Length;
                    protoMsg.VesselId = vesselId;

                    MessageQueuer.SendToClient<VesselSrvMsg>(client, protoMsg);
                }
            }

            if (allVessels.Count > 0)
                LunaLog.Debug($"Sending {client.PlayerName} {vesselsToSend.Count} vessels");
        }
    }
}
