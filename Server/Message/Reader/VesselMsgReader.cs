using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using LunaServer.System;

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
                case VesselMessageType.Remove:
                    HandleVesselRemove(client, message);
                    break;
                case VesselMessageType.Change:
                    MessageQueuer.RelayMessage<VesselSrvMsg>(client, message);
                    break;
                case VesselMessageType.Update:
                    VesselRelaySystem.HandleVesselMessage(client, message);
                    break;
                case VesselMessageType.Position:
                    VesselRelaySystem.HandleVesselMessage(client, message);
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
            var data = (VesselRemoveMsgData) message;

            //Don't care about the Subspace on the server.
            LunaLog.Debug($"Removing vessel {data.VesselId} from {client.PlayerName}");

            Universe.RemoveFromUniverse(Path.Combine(ServerContext.UniverseDirectory, "Vessels", data.VesselId + ".txt"));
            VesselContext.RemovedVessels.Add(data.VesselId);

            //Relay the message.
            if (data.Broadcast)
                MessageQueuer.SendToAllClients<VesselSrvMsg>(data);
            else
                MessageQueuer.RelayMessage<VesselSrvMsg>(client, data);
        }

        private static void HandleVesselProto(ClientStructure client, VesselBaseMsgData message)
        {
            var msgData = (VesselProtoMsgData) message;
            
            if (VesselContext.RemovedVessels.Contains(msgData.VesselId)) return;

            var path = Path.Combine(ServerContext.UniverseDirectory, "Vessels", msgData.VesselId + ".txt");

            if (!File.Exists(path))
                LunaLog.Debug($"Saving vessel {msgData.VesselId} from {client.PlayerName}");

            FileHandler.WriteToFile(path, msgData.VesselData);

            if (!FileHandler.ReadFileText(path).Contains("PotatoRoid"))
                LunaLog.Debug($"Received PROTO vessel: {msgData.VesselId} from {client.PlayerName}");

            VesselRelaySystem.HandleVesselMessage(client, message);
        }

        private static void HandleVesselsRequest(ClientStructure client, IMessageData messageData)
        {
            var sendVesselCount = 0;
            var cachedVesselCount = 0;
            var clientRequested = (messageData as VesselsRequestMsgData)?.RequestList ?? new string[0];

            var vesselList = new List<KeyValuePair<string, byte[]>>();

            foreach (var file in FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels")))
            {
                var vesselId = Path.GetFileNameWithoutExtension(file);
                var vesselData = FileHandler.ReadFile(file);
                var vesselObject = Common.CalculateSha256Hash(vesselData);
                if (clientRequested.Contains(vesselObject))
                {
                    sendVesselCount++;
                    vesselList.Add(new KeyValuePair<string, byte[]>(vesselId, vesselData));
                }
                else
                {
                    cachedVesselCount++;
                }
            }

            MessageQueuer.SendToClient<VesselSrvMsg>(client, new VesselsReplyMsgData {VesselsData = vesselList.ToArray()});
            LunaLog.Debug($"Sending {client.PlayerName} {sendVesselCount} vessels, cached: {cachedVesselCount}...");
        }

        private static void HandleVesselListRequest(ClientStructure client)
        {
            MessageQueuer.SendToClient<VesselSrvMsg>(client, new VesselListReplyMsgData
            {
                Vessels = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels"))
                        .Select(Common.CalculateSha256Hash)
                        .ToArray()
            });
        }
    }
}
