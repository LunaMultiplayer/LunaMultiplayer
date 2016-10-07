using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.ModApi
{
    public class ModApiMessageHandler : SubSystem<ModApiSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as ModMsgData;
            if (msgData == null) return;

            var modName = msgData.ModName;
            var modData = msgData.Data;
            lock (System.EventLock)
            {
                if (System.UpdateQueue.ContainsKey(modName))
                    System.UpdateQueue[modName].Enqueue(modData);

                if (System.FixedUpdateQueue.ContainsKey(modName))
                    System.FixedUpdateQueue[modName].Enqueue(modData);

                if (System.RegisteredRawMods.ContainsKey(modName))
                    System.RegisteredRawMods[modName](modData);
            }
        }
    }
}