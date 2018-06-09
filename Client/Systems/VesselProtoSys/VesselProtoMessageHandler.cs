using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselStore;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;

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
                case VesselMessageType.Proto:
                    HandleVesselProto((VesselProtoMsgData)msgData);
                    break;
                default:
                    LunaLog.LogError($"[LMP]: Cannot handle messages of type: {msgData.VesselMessageType} in VesselProtoMessageHandler");
                    break;
            }
        }

        private static void HandleVesselProto(VesselProtoMsgData messageData)
        {
            if (!VesselRemoveSystem.Singleton.VesselWillBeKilled(messageData.VesselId) && messageData.VesselId != Guid.Empty)
            {
                VesselsProtoStore.HandleVesselProtoData(messageData.Data, messageData.NumBytes, messageData.VesselId, messageData.ForceReload, messageData.GameTime);
            }
        }
    }
}
