using LunaClient.Base;
using LunaClient.Base.Interface;
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

            var modName = msgData.ModName;

            //Clone it as after that the mesage will be recycled
            //TODO: can this be improved to avoid garbage?
            var modData = Common.TrimArray(msgData.Data.Clone() as byte[], msgData.NumBytes);
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