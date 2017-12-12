using LunaCommon;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Server.Message.Reader
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
                case VesselMessageType.ProtoReliable:
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
            var msgData = (VesselProtoBaseMsgData)message;

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
        private static void RewriteVesselProtoPositionInfo(VesselBaseMsgData message)
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

            //var updatedText = UpdateProtoVesselFileWithNewPositionData(protoVesselLines, msgData);

            //FileHandler.WriteToFile(path, updatedText);
        }

        /// <summary>
        /// Updates the proto vessel file with the values we received about a position of a vessel
        /// </summary>
        private static string UpdateProtoVesselFileWithNewPositionData(string[] protoVesselLines, VesselPositionMsgData msgData)
        {
            var fullText = string.Join(Environment.NewLine, protoVesselLines);

            var regex = new Regex("lat = (.*)");
            fullText = regex.Replace(fullText, msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture));

            regex = new Regex("lon = (.*)");
            fullText = regex.Replace(fullText, msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture));

            regex = new Regex("alt = (.*)");
            fullText = regex.Replace(fullText, msgData.LatLonAlt[0].ToString(CultureInfo.InvariantCulture));
            
            regex = new Regex("nrm = (.*)");
            fullText = regex.Replace(fullText, msgData.NormalVector[0].ToString(CultureInfo.InvariantCulture) + "," + 
                msgData.NormalVector[1].ToString(CultureInfo.InvariantCulture) + "," + 
                msgData.NormalVector[2].ToString(CultureInfo.InvariantCulture));

            regex = new Regex("rot = (.*)");
            fullText = regex.Replace(fullText, msgData.SrfRelRotation[0].ToString(CultureInfo.InvariantCulture) + "," + 
                msgData.SrfRelRotation[1].ToString(CultureInfo.InvariantCulture) + "," + 
                msgData.SrfRelRotation[2].ToString(CultureInfo.InvariantCulture) + "," + 
                msgData.SrfRelRotation[3].ToString(CultureInfo.InvariantCulture));

            regex = new Regex("CoM = (.*)");
            fullText = regex.Replace(fullText, msgData.Com[0].ToString(CultureInfo.InvariantCulture) + "," + 
                msgData.Com[1].ToString(CultureInfo.InvariantCulture) + "," + 
                msgData.Com[2].ToString(CultureInfo.InvariantCulture));

            regex = new Regex("INC = (.*)"); //inclination
            fullText = regex.Replace(fullText, msgData.Orbit[0].ToString(CultureInfo.InvariantCulture));
            regex = new Regex("ECC = (.*)"); //eccentricity
            fullText = regex.Replace(fullText, msgData.Orbit[1].ToString(CultureInfo.InvariantCulture));
            regex = new Regex("SMA = (.*)"); //semiMajorAxis
            fullText = regex.Replace(fullText, msgData.Orbit[2].ToString(CultureInfo.InvariantCulture));
            regex = new Regex("LAN = (.*)"); //LAN
            fullText = regex.Replace(fullText, msgData.Orbit[3].ToString(CultureInfo.InvariantCulture));
            regex = new Regex("LPE = (.*)"); //argumentOfPeriapsis
            fullText = regex.Replace(fullText, msgData.Orbit[4].ToString(CultureInfo.InvariantCulture));
            regex = new Regex("MNA = (.*)"); //meanAnomalyAtEpoch
            fullText = regex.Replace(fullText, msgData.Orbit[5].ToString(CultureInfo.InvariantCulture));
            regex = new Regex("EPH = (.*)"); //epoch
            fullText = regex.Replace(fullText, msgData.Orbit[6].ToString(CultureInfo.InvariantCulture));
            regex = new Regex("REF = (.*)"); //referenceBody.flightGlobalsIndex
            fullText = regex.Replace(fullText, msgData.Orbit[7].ToString(CultureInfo.InvariantCulture));

            return fullText;
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
