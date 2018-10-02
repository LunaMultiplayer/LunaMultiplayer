using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System.Collections.Concurrent;

namespace LmpClient.Systems.ShareScience
{
    public class ShareScienceMessageHandler : SubSystem<ShareScienceSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.ScienceUpdate) return;

            if (msgData is ShareProgressScienceMsgData data)
            {
                var science = data.Science; //create a copy of the science value so it will not change in the future.
                LunaLog.Log($"Queue ScienceUpdate with: {science}");
                System.QueueAction(() =>
                {
                    ScienceUpdate(science);
                });
            }
        }

        private static void ScienceUpdate(float science)
        {
            System.SetScienceWithoutTriggeringEvent(science);
            LunaLog.Log($"ScienceUpdate received - science changed to: {science}");
        }
    }
}
