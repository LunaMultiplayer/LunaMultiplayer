using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.ShareReputation
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
                System.QueueAction(() =>
                {
                    ReputationUpdate(reputation);
                });
            }
        }

        private static void ReputationUpdate(float reputation)
        {
            System.StartIgnoringEvents();
            Reputation.Instance.SetReputation(reputation, TransactionReasons.None);
            System.StopIgnoringEvents();
            LunaLog.Log($"ReputationUpdate received - reputation changed to: {reputation}");
        }
    }
}
