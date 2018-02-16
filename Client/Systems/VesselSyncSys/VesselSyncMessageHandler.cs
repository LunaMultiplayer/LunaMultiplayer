using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselSyncSys
{
    public class VesselSyncMessageHandler : SubSystem<VesselSyncSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            throw new Exception("Vessel SYNC messages are not handled on client-side!");
        }
    }
}
