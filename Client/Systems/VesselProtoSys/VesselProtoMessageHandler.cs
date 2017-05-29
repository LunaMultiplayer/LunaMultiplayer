using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
                    Debug.LogError($"[LMP]: Cannot handle messages of type: {msgData.VesselMessageType} in VesselProtoMessageHandler");
                    break;
            }
        }

        private static void HandleVesselProto(VesselProtoMsgData messageData)
        {
            if (!System.ProtoSystemBasicReady || VesselCommon.UpdateIsForOwnVessel(messageData.VesselId))
            {
                return;
            }

            HandleVesselProtoData(messageData.VesselData, messageData.VesselId);
        }

        private static void HandleVesselResponse(VesselsReplyMsgData messageData)
        {
            foreach (var vesselDataKv in messageData.VesselsData)
            {
                HandleVesselProtoData(vesselDataKv.Value, new Guid(vesselDataKv.Key));
            }

            SystemsContainer.Get<MainSystem>().NetworkState = ClientState.VesselsSynced;
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
                        var vesselIdString = Common.ConvertConfigStringToGuidString(vesselNode.GetValue("pid"));
                        var vesselId = new Guid(vesselIdString);
                        if (vesselBytes.Length != 0 && vesselId != Guid.Empty)
                        {
                            System.AllPlayerVessels.TryAdd(vesselId, new VesselProtoUpdate
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
                else
                {
                    requestedObjects.Add(serverVessel);
                }
            }

            //Request the vessel data that we don't have.
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>
                (new VesselsRequestMsgData { RequestList = requestedObjects.ToArray() }));
        }

        /// <summary>
        /// In this method we get the new vessel data and set it to the dictionary of all the player vessels.
        /// We set it as UNLOADED as perhaps vessel data has changed.
        /// We also do all of this asynchronously to improve performance
        /// </summary>
        private static void HandleVesselProtoData(byte[] vesselData, Guid vesselId)
        {
            new Thread(() =>
            {
                UniverseSyncCache.QueueToCache(vesselData);
                var vesselNode = ConfigNodeSerializer.Deserialize(vesselData);
                var configGuid = vesselNode?.GetValue("pid");
                if (!string.IsNullOrEmpty(configGuid) && vesselId.ToString() == Common.ConvertConfigStringToGuidString(configGuid))
                {
                    var vesselProtoUpdate = new VesselProtoUpdate
                    {
                        VesselId = vesselId,
                        VesselNode = vesselNode,
                        Loaded = false
                    };

                    if (System.AllPlayerVessels.ContainsKey(vesselId))
                    {
                        //Vessel exists so replace it
                        System.AllPlayerVessels[vesselId] = vesselProtoUpdate;
                    }
                    else
                    {
                        System.AllPlayerVessels.TryAdd(vesselId, vesselProtoUpdate);
                    }
                }
            }).Start();
        }
    }
}
