using LunaCommon;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using LunaServer.Settings;
using LunaServer.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaServer.Message.Reader
{
    public class VesselMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as VesselBaseMsgData;
            switch (message?.VesselMessageType)
            {
                case VesselMessageType.ListRequest:
                    HandleVesselListRequest(client);
                    break;
                case VesselMessageType.VesselsRequest:
                    HandleVesselsRequest(client, messageData);
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
                    if (!GeneralSettings.SettingsStore.ShowVesselsInThePast || 
                        client.Subspace == WarpContext.LatestSubspace && GeneralSettings.SettingsStore.ShowVesselsInThePast)
                        RewriteVesselProtoPositionInfo(message);
                    break;
                case VesselMessageType.Flightstate:
                    VesselRelaySystem.HandleVesselMessage(client, message);
                    break;
                default:
                    throw new NotImplementedException("Warp Type not implemented");
            }
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

            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.VesselId}.txt");

            if (!File.Exists(path))
                LunaLog.Debug($"Saving vessel {msgData.VesselId} from {client.PlayerName}");

            FileHandler.WriteToFile(path, msgData.VesselData);

            VesselRelaySystem.HandleVesselMessage(client, message);
        }

        /// <summary>
        /// We received a position information from a player in the latest subspace.
        /// Then we rewrite the vesselproto with that last position so players that connect later receive an update vesselproto
        /// </summary>
        private void RewriteVesselProtoPositionInfo(VesselBaseMsgData message)
        {
            var msgData = (VesselPositionMsgData)message;
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            //If someone is updating this vessel, ignore this as the player with the update lock will send the whole
            //protovessel at a interval
            if (LockSystem.LockQuery.UpdateLockExists(msgData.VesselId)) return;

            //Now we are sure that the message we received is for a vessel that is stranded somewhere and nobody is either
            //controlling it or near it...
            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.VesselId}.txt");
            if (!File.Exists(path)) return; //didn't found a vessel to rewrite so quit

            var protoVesselLines = FileHandler.ReadFileLines(path);

            UpdateProtoVesselFileWithNewPositionData(protoVesselLines, msgData);

            FileHandler.WriteToFile(path, string.Join(Environment.NewLine, protoVesselLines));
        }

        private static void UpdateProtoVesselFileWithNewPositionData(string[] protoVesselLines, VesselPositionMsgData msgData)
        {
            //TODO: Here in the vessel file we should update all this fields according to the msgData:

            /*
                lat = -0.047961032043514852
                lon = -74.722209730407329
                alt = 69.873375568306074
                hgt = 0.797961473
                nrm = -0.00204212964,-0.0692077577,-0.997600377
                rot = 0.120999679,-0.116582111,-0.731465757,0.660852194
                CoM = 3.93167138E-05,-0.654579401,0.28330332
                ORBIT
                {
	                SMA = 300817.15359923861
	                ECC = 0.99479860226031658
	                INC = 0.047954560954079489
	                LPE = 90.023932404825203
	                LAN = 141.35674401115426
	                MNA = 3.141592620319944
	                EPH = 2161.1050583995857
	                REF = 1
                }
            */
        }

        private static void HandleVesselDock(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselDockMsgData)message;

            LunaLog.Debug($"Docking message received! Dominant vessel: {msgData.DominantVesselId}");

            if (VesselContext.RemovedVessels.Contains(msgData.WeakVesselId)) return;

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", $"{msgData.DominantVesselId}.txt");
            if (!File.Exists(path))
                LunaLog.Debug($"Saving vessel {msgData.DominantVesselId} from {client.PlayerName}");
            FileHandler.WriteToFile(path, msgData.FinalVesselData);

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

        private static void HandleVesselsRequest(ClientStructure client, IMessageData messageData)
        {
            var sendVesselCount = 0;
            var cachedVesselCount = 0;
            var clientRequested = (messageData as VesselsRequestMsgData)?.RequestList ?? new string[0];

            var vesselList = new List<KeyValuePair<Guid, byte[]>>();

            foreach (var file in FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels")))
            {
                var vesselId = Path.GetFileNameWithoutExtension(file);
                var vesselData = FileHandler.ReadFile(file);
                var vesselObject = Common.CalculateSha256Hash(vesselData);
                if (clientRequested.Contains(vesselObject))
                {
                    sendVesselCount++;
                    vesselList.Add(new KeyValuePair<Guid, byte[]>(new Guid(vesselId), vesselData));
                }
                else
                {
                    cachedVesselCount++;
                }
            }

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselsReplyMsgData>();
            msgData.VesselsData = vesselList.ToArray();

            MessageQueuer.SendToClient<VesselSrvMsg>(client, msgData);
            LunaLog.Debug($"Sending {client.PlayerName} {sendVesselCount} vessels, cached: {cachedVesselCount}...");
        }

        private static void HandleVesselListRequest(ClientStructure client)
        {
            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselListReplyMsgData>();
            msgData.Vessels = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels"))
                .Select(Common.CalculateSha256Hash)
                .ToArray();

            MessageQueuer.SendToClient<VesselSrvMsg>(client, msgData);
        }
    }
}
