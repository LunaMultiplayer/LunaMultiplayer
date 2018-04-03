using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Events;
using LunaCommon;
using LunaCommon.Message.Data;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.ModApi
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
