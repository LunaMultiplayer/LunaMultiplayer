using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Network;
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageHandler : SubSystem<VesselProtoSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselBaseMsgData;
            if (msgData == null) return;

            switch (msgData.VesselMessageType)
            {
                case VesselMessageType.LIST_REPLY:
                    HandleVesselList((VesselListReplyMsgData)messageData);
                    break;
                case VesselMessageType.VESSELS_REPLY:
                    HandleVesselResponse((VesselsReplyMsgData)messageData);
                    break;
                case VesselMessageType.PROTO:
                    HandleVesselProto((VesselProtoMsgData)messageData);
                    break;
                default:
                    Debug.LogError($"[LMP]: Cannot handle messages of type: {msgData.VesselMessageType} in VesselProtoMessageHandler");
                    break;
            }
        }
        
        private static void HandleVesselProto(VesselProtoMsgData messageData)
        {
            HandleVesselProtoData(messageData.VesselData, messageData.VesselId.ToString(), messageData.Subspace);
        }

        private static void HandleVesselResponse(VesselsReplyMsgData messageData)
        {
            foreach (var vesselDataKv in messageData.VesselsData)
            {
                HandleVesselProtoData(vesselDataKv.Value, vesselDataKv.Key, 0);
            }
            MainSystem.Singleton.NetworkState = ClientState.VESSELS_SYNCED;
        }

        private static void HandleVesselList(VesselListReplyMsgData messageData)
        {
            var serverVessels = new List<string>(messageData.Vessels);
            var cacheObjects = new List<string>(UniverseSyncCache.Singleton.GetCachedObjects());
            var requestedObjects = new List<string>();
            foreach (var serverVessel in serverVessels)
            {
                if (!cacheObjects.Contains(serverVessel))
                {
                    requestedObjects.Add(serverVessel);
                }
                else
                {
                    //Try to get it from cache...
                    var vesselBytes = UniverseSyncCache.Singleton.GetFromCache(serverVessel);
                    var vesselNode = ConfigNodeSerializer.Singleton.Deserialize(vesselBytes);
                    if (vesselNode != null)
                    {
                        var vesselIdString = Common.ConvertConfigStringToGuidString(vesselNode.GetValue("pid"));
                        var vesselId = new Guid(vesselIdString);
                        if (vesselBytes.Length != 0 && vesselId != Guid.Empty)
                        {
                            System.AllPlayerVessels.Add(new VesselProtoUpdate
                            {
                                Loaded = false,
                                VesselId = vesselId,
                                VesselNode = vesselNode
                            });
                        }
                        else
                        {
                            Debug.LogError($"[LMP]: Cached object {serverVessel} is damaged");
                            requestedObjects.Add(serverVessel);
                        }
                    }
                }
            }

            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>
                (new VesselsRequestMsgData { RequestList = requestedObjects.ToArray() }));
        }

        private static void HandleVesselProtoData(byte[] vesselData, string vesselId, int subspace)
        {
            UniverseSyncCache.Singleton.QueueToCache(vesselData);
            var vesselNode = ConfigNodeSerializer.Singleton.Deserialize(vesselData);
            if (vesselNode != null)
            {
                var configGuid = vesselNode.GetValue("pid");
                if (!string.IsNullOrEmpty(configGuid) && vesselId == Common.ConvertConfigStringToGuidString(configGuid))
                {
                    var vesselProtoUpdate = new VesselProtoUpdate
                    {
                        VesselId = new Guid(vesselId),
                        VesselNode = vesselNode,
                        Loaded = false
                    };

                    var oldVessel = System.AllPlayerVessels.FirstOrDefault(v => v.VesselId.ToString() == vesselId);
                    if (oldVessel != null)
                    {
                        //Vessel exists so replace it
                        var index = System.AllPlayerVessels.IndexOf(oldVessel);
                        System.AllPlayerVessels[index] = vesselProtoUpdate;
                    }
                    else
                    {
                        System.AllPlayerVessels.Add(vesselProtoUpdate);
                    }
                    VesselWarpSystem.Singleton.AddUpdateVesselSubspace(vesselProtoUpdate.VesselId, subspace);
                }
                else
                {
                    Debug.LogError($"[LMP]: Failed to load vessel {vesselId}!");
                }
            }
            else
            {
                Debug.LogError($"[LMP]: Failed to load vessel {vesselId}!");
            }
        }
    }
}
