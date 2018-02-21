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
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                        VesselFileUpdater.WritePositionDataToFile(message);
                    break;
                case VesselMessageType.Flightstate:
                    VesselRelaySystem.HandleVesselMessage(client, message);
                    break;
                case VesselMessageType.Update:
                    VesselFileUpdater.WriteUpdateDataToFile(message);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, message);
                    break;
                case VesselMessageType.Resource:
                    VesselFileUpdater.WriteResourceDataToFile(message);
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, message);
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
        }

        private static void HandleVesselRemove(ClientStructure client, VesselBaseMsgData message)
        {
            var data = (VesselRemoveMsgData) message;

            if (LockSystem.LockQuery.ControlLockExists(data.VesselId) && !LockSystem.LockQuery.ControlLockBelongsToPlayer(data.VesselId, client.PlayerName))
                return;

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{data.VesselId}.txt");
            if (FileHandler.FileExists(path))
            {
                LunaLog.Debug($"Removing vessel {data.VesselId} from {client.PlayerName}");
                Universe.RemoveFromUniverse(Path.Combine(ServerContext.UniverseDirectory, "Vessels",
                    $"{data.VesselId}.txt"));
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

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.Vessel.VesselId}.txt");

            if (!File.Exists(path))
                LunaLog.Debug($"Saving vessel {msgData.Vessel.VesselId} from {client.PlayerName}. Bytes: {msgData.Vessel.NumBytes}");

            FileHandler.WriteToFile(path, msgData.Vessel.Data, msgData.Vessel.NumBytes);

            MessageQueuer.RelayMessage<VesselSrvMsg>(client, msgData);
        }

        private static void HandleVesselDock(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselDockMsgData) message;

            LunaLog.Debug($"Docking message received! Dominant vessel: {msgData.DominantVesselId}");

            if (VesselContext.RemovedVessels.Contains(msgData.WeakVesselId)) return;

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.DominantVesselId}.txt");
            if (!File.Exists(path))
                LunaLog.Debug($"Saving vessel {msgData.DominantVesselId} from {client.PlayerName}");
            FileHandler.WriteToFile(path, msgData.FinalVesselData, msgData.NumBytes);

            //Now remove the weak vessel
            LunaLog.Debug($"Removing weak docked vessel {msgData.WeakVesselId}");
            Universe.RemoveFromUniverse(Path.Combine(ServerContext.UniverseDirectory, "Vessels",
                $"{msgData.WeakVesselId}.txt"));
            VesselContext.RemovedVessels.Add(msgData.WeakVesselId);

            MessageQueuer.RelayMessage<VesselSrvMsg>(client, msgData);

            //Tell all clients to remove the weak vessel
            var removeMsgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
            removeMsgData.VesselId = msgData.WeakVesselId;

            MessageQueuer.SendToAllClients<VesselSrvMsg>(removeMsgData);
        }

        private static void HandleVesselsSync(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselSyncMsgData) message;

            var vesselsToSend = GetCurrentVesselIds().Except(msgData.VesselIds).ToArray();
            foreach (var vesselId in vesselsToSend)
            {
                var vesselData = FileHandler.ReadFile(Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{vesselId}.txt"));
                if (vesselData.Length > 0)
                {
                    var protoMsg = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselProtoMsgData>();
                    protoMsg.Vessel.Data = vesselData;
                    protoMsg.Vessel.NumBytes = vesselData.Length;
                    protoMsg.Vessel.VesselId = vesselId;

                    MessageQueuer.SendToClient<VesselSrvMsg>(client, protoMsg);
                }
            }

            if (vesselsToSend.Length > 0)
                LunaLog.Debug($"Sending {client.PlayerName} {vesselsToSend.Length} vessels");
        }

        private static IEnumerable<Guid> GetCurrentVesselIds()
        {
            var vesselIds = new List<Guid>();
            foreach (var file in FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels")))
            {
                if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out var vesselId))
                {
                    vesselIds.Add(vesselId);
                }
            }

            return vesselIds;
        }
    }
}
