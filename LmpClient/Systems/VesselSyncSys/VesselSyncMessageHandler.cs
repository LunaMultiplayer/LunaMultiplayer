using System;
using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselSyncSys
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
