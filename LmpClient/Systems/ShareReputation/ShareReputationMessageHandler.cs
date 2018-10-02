using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Systems.ShareCareer;
using LmpCommon.Message.Data.ShareProgress;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using System.Collections.Concurrent;

namespace LmpClient.Systems.ShareReputation
{
    public class ShareReputationMessageHandler : SubSystem<ShareReputationSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.ReputationUpdate) return;

            if (msgData is ShareProgressReputationMsgData data)
            {
                var reputation = data.Reputation; //create a copy of the reputation value so it will not change in the future.
                LunaLog.Log($"Queue ReputationUpdate with: {reputation}");
                ShareCareerSystem.Singleton.QueueAction(() =>
                {
                    ReputationUpdate(reputation);
                });
            }
        }

        private static void ReputationUpdate(float reputation)
        {
            System.SetReputationWithoutTriggeringEvent(reputation);
            LunaLog.Log($"ReputationUpdate received - reputation changed to: {reputation}");
        }
    }
}
