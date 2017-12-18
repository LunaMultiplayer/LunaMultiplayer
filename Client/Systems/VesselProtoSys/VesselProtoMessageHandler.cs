using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.VesselRemoveSys;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;
using UniLinq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoMessageHandler : SubSystem<VesselProtoSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselBaseMsgData msgData)) return;

            switch (msgData.VesselMessageType)
            {
                case VesselMessageType.ListReply:
                    HandleVesselList((VesselListReplyMsgData)msgData);
                    break;
                case VesselMessageType.VesselsReply:
                    HandleVesselResponse((VesselsReplyMsgData)msgData);
                    break;
                case VesselMessageType.Proto:
                case VesselMessageType.ProtoReliable:
                    HandleVesselProto((VesselProtoBaseMsgData)msgData);
                    break;
                default:
                    LunaLog.LogError($"[LMP]: Cannot handle messages of type: {msgData.VesselMessageType} in VesselProtoMessageHandler");
                    break;
            }
        }

        private static void HandleVesselProto(VesselProtoBaseMsgData messageData)
        {
            if (!SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(messageData.Vessel.VesselId))
                VesselsProtoStore.HandleVesselProtoData(messageData.Vessel.Data, messageData.Vessel.NumBytes, messageData.Vessel.VesselId);
        }

        private static void HandleVesselResponse(VesselsReplyMsgData messageData)
        {
            //We read the vessels syncronously so when we start the game we have the dictionary of all the vessels already loaded
            for (var i = 0; i < messageData.VesselsCount; i++)
            {
                if (!SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(messageData.VesselsData[i].VesselId))
                    VesselsProtoStore.HandleVesselProtoData(messageData.VesselsData[i].Data, messageData.VesselsData[i].NumBytes, messageData.VesselsData[i].VesselId, true);
            }

            MainSystem.NetworkState = ClientState.VesselsSynced;
        }

        /// <summary>
        /// Here we receive the vessel list msg from the server. And we request all the vesseslw e don't have.
        /// Before we used a cache system but it was a bad idea as protovessels change very often as they hold the orbit data, etc.
        /// </summary>
        private static void HandleVesselList(VesselListReplyMsgData messageData)
        {
            //Request the vessel data that we don't have.
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselsRequestMsgData>();

            //Clone the array as the VesselListReplyMsgData will be recycled!
            msgData.RequestList = messageData.Vessels.Select(v=> v.Clone() as string).ToArray();
            msgData.RequestCount = messageData.VesselsCount;

            System.MessageSender.SendMessage(msgData);
        }
    }
}
