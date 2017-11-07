using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageHandler : SubSystem<VesselProtoSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is VesselBaseMsgData msgData)) return;

            switch (msgData.VesselMessageType)
            {
                case VesselMessageType.ListReply:
                    HandleVesselList((VesselListReplyMsgData)messageData);
                    break;
                case VesselMessageType.VesselsReply:
                    HandleVesselResponse((VesselsReplyMsgData)messageData);
                    break;
                case VesselMessageType.Proto:
                    HandleVesselProto((VesselProtoMsgData)messageData);
                    break;
                default:
                    LunaLog.LogError($"[LMP]: Cannot handle messages of type: {msgData.VesselMessageType} in VesselProtoMessageHandler");
                    break;
            }
        }

        private static void HandleVesselProto(VesselProtoMsgData messageData)
        {
            if (!SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(messageData.VesselId))
                System.HandleVesselProtoData(messageData.VesselData, messageData.VesselId);
        }

        private static void HandleVesselResponse(VesselsReplyMsgData messageData)
        {
            foreach (var vesselDataKv in messageData.VesselsData)
            {
                if (!SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(vesselDataKv.Key))
                    System.HandleVesselProtoData(vesselDataKv.Value, vesselDataKv.Key);
            }

            MainSystem.NetworkState = ClientState.VesselsSynced;
        }

        /// <summary>
        /// Here we receive the vessel list msg from the server.We rty to get the vessels from the cache and if
        /// it fails or we don't have it in the cache we request that vessel info to the server.
        /// </summary>
        private static void HandleVesselList(VesselListReplyMsgData messageData)
        {
            var serverVessels = new List<string>(messageData.Vessels);
            var cacheObjects = new List<string>(UniverseSyncCache.GetCachedObjects());
            var requestedObjects = new List<string>();
            foreach (var serverVessel in serverVessels)
            {
                if (cacheObjects.Contains(serverVessel))
                {
                    //Try to get it from cache...
                    var vesselBytes = UniverseSyncCache.GetFromCache(serverVessel);
                    var vesselNode = ConfigNodeSerializer.Deserialize(vesselBytes);
                    if (vesselNode != null)
                    {
                        var vesselId = Common.ConvertConfigStringToGuid(vesselNode.GetValue("pid"));
                        if (vesselBytes.Length != 0 && vesselId != Guid.Empty)
                        {
                            var update = new VesselProtoUpdate(vesselNode, vesselId);
                            //We set this to false as the game still has not started and we will call LoadVesselsIntoGame that runs trough all vessels
                            //and does not care about if they must be reloaded or not.
                            update.NeedsToBeReloaded = false;
                            if (update.ProtoVessel != null)
                                System.AllPlayerVessels.TryAdd(vesselId, update);
                        }
                        else
                        {
                            LunaLog.LogError($"[LMP]: Cached object {serverVessel} is damaged");
                            requestedObjects.Add(serverVessel);
                        }
                    }
                }
                else
                {
                    requestedObjects.Add(serverVessel);
                }
            }

            //Request the vessel data that we don't have.
            System.MessageSender.SendMessage(new VesselsRequestMsgData { RequestList = requestedObjects.ToArray() });
        }
    }
}
