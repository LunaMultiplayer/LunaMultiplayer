using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Events;
using LmpCommon;
using LmpCommon.Message.Data;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.ModApi
{
    public class ModApiMessageHandler : SubSystem<ModApiSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ModMsgData msgData)) return;

            var modData = Common.TrimArray(msgData.Data, msgData.NumBytes);
            ModApiEvent.onModMessageReceived.Fire(msgData.ModName, modData);
        }
    }
}
