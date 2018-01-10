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
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Message.Reader
{
    public class VesselMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as VesselBaseMsgData;
            switch (message?.VesselMessageType)
            {
                case VesselMessageType.VesselsRequest:
                    HandleVesselsRequest(client);
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
                        VesselFileUpdater.RewriteVesselFile(message);
                    break;
                case VesselMessageType.Flightstate:
                    VesselRelaySystem.HandleVesselMessage(client, message);
                    break;
                case VesselMessageType.Update:
                    HandleVesselUpdate(message);
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
        }

        private static void HandleVesselUpdate(VesselBaseMsgData message)
        {
            VesselFileUpdater.RewriteVesselFile(message);
            MessageQueuer.SendToAllClients<VesselSrvMsg>(message);
        }

        private static void HandleVesselRemove(ClientStructure client, VesselBaseMsgData message)
        {
            var data = (VesselRemoveMsgData)message;

            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(data.VesselId, client.PlayerName) &&
                !LockSystem.LockQuery.UpdateLockBelongsToPlayer(data.VesselId, client.PlayerName))
                return;

            //Don't care about the Subspace on the server.
            LunaLog.Debug($"Removing vessel {data.VesselId} from {client.PlayerName}");

            Universe.RemoveFromUniverse(Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{data.VesselId}.txt"));
            VesselContext.RemovedVessels.Add(data.VesselId);

            //Relay the message.
            MessageQueuer.SendToAllClients<VesselSrvMsg>(data);
        }

        private static void HandleVesselProto(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselProtoMsgData)message;

            if (VesselContext.RemovedVessels.Contains(msgData.Vessel.VesselId)) return;

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.Vessel.VesselId}.txt");

            if (!File.Exists(path))
                LunaLog.Debug($"Saving vessel {msgData.Vessel.VesselId} from {client.PlayerName}");

            FileHandler.WriteToFile(path, msgData.Vessel.Data, msgData.Vessel.NumBytes);

            VesselRelaySystem.HandleVesselMessage(client, message);
        }





        private static void HandleVesselDock(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselDockMsgData)message;

            LunaLog.Debug($"Docking message received! Dominant vessel: {msgData.DominantVesselId}");

            if (VesselContext.RemovedVessels.Contains(msgData.WeakVesselId)) return;

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.DominantVesselId}.txt");
            if (!File.Exists(path))
                LunaLog.Debug($"Saving vessel {msgData.DominantVesselId} from {client.PlayerName}");
            FileHandler.WriteToFile(path, msgData.FinalVesselData, msgData.NumBytes);

            //Now remove the weak vessel
            LunaLog.Debug($"Removing weak docked vessel {msgData.WeakVesselId}");
            Universe.RemoveFromUniverse(Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.WeakVesselId}.txt"));
            VesselContext.RemovedVessels.Add(msgData.WeakVesselId);

            MessageQueuer.RelayMessage<VesselSrvMsg>(client, msgData);

            //Tell all clients to remove the weak vessel
            var removeMsgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
            removeMsgData.VesselId = msgData.WeakVesselId;

            MessageQueuer.SendToAllClients<VesselSrvMsg>(removeMsgData);
        }

        private static void HandleVesselsRequest(ClientStructure client)
        {
            var sendVesselCount = 0;

            var vesselList = new List<VesselInfo>();

            foreach (var file in FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels")))
            {
                var vesselData = FileHandler.ReadFile(file);
                if (Guid.TryParse(Path.GetFileNameWithoutExtension(file), out var vesselId))
                {
                    sendVesselCount++;
                    vesselList.Add(new VesselInfo
                    {
                        Data = vesselData,
                        NumBytes = vesselData.Length,
                        VesselId = vesselId
                    });
                }
            }

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselsReplyMsgData>();
            msgData.VesselsData = vesselList.ToArray();
            msgData.VesselsCount = vesselList.Count;

            MessageQueuer.SendToClient<VesselSrvMsg>(client, msgData);
            LunaLog.Debug($"Sending {client.PlayerName} {sendVesselCount} vessels");
        }
    }
}
