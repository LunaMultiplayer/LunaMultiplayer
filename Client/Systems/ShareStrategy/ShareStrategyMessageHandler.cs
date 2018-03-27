using System;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;
using Contracts;
using LunaClient.Systems.ShareFunds;
using LunaClient.Systems.ShareReputation;
using LunaClient.Systems.ShareScience;
using LunaClient.Utilities;
using Strategies;

namespace LunaClient.Systems.ShareStrategy
{
    public class ShareStrategyMessageHandler : SubSystem<ShareStrategySystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ShareProgressBaseMsgData msgData)) return;
            if (msgData.ShareProgressMessageType != ShareProgressMessageType.StrategyUpdate) return;

            if (msgData is ShareProgressStrategyMsgData data)
            {
                var strategy = new StrategyInfo(data.Strategy); //create a copy of the strategyInfo object so it will not change in the future.
                LunaLog.Log($"Queue StrategyUpdate with: {strategy.Title}");
                System.QueueAction(() =>
                {
                    StrategyUpdate(strategy);
                });
            }
        }

        private static void StrategyUpdate(StrategyInfo strategyInfo)
        {
            //TODO: This is not working correctly because the strategy.Load() method is not working as excpected.
            //var incomingStrategy = ConvertByteArrayToStrategy(strategyInfo.Data, strategyInfo.NumBytes);
            //if (incomingStrategy == null) return;

            //Don't listen to these events for the time this message is processing.
            System.StartIgnoringEvents();
            ShareFundsSystem.Singleton.StartIgnoringEvents();
            ShareScienceSystem.Singleton.StartIgnoringEvents();
            ShareReputationSystem.Singleton.StartIgnoringEvents();

            var strategyIndex = StrategySystem.Instance.Strategies.FindIndex(strat => strat.Title == strategyInfo.Title);
            if (strategyIndex != -1)
            {
                if (strategyInfo.IsActive)
                {
                    StrategySystem.Instance.Strategies[strategyIndex].Factor = strategyInfo.Factor;
                    StrategySystem.Instance.Strategies[strategyIndex].Activate();
                    LunaLog.Log($"StrategyUpdate received - strategy activated: {strategyInfo.Title}  - with factor: {strategyInfo.Factor}");
                }
                else
                {
                    StrategySystem.Instance.Strategies[strategyIndex].Factor = strategyInfo.Factor;
                    StrategySystem.Instance.Strategies[strategyIndex].Deactivate();
                    LunaLog.Log($"StrategyUpdate received - strategy deactivated: {strategyInfo.Title}  - with factor: {strategyInfo.Factor}");
                }
            }

            //TODO: Refresh the strategy building ui.

            // Listen to the events again.
            //Restore funds, science and reputation in case the contract action changed some of that.
            ShareFundsSystem.Singleton.StopIgnoringEvents(true);
            ShareScienceSystem.Singleton.StopIgnoringEvents(true);
            ShareReputationSystem.Singleton.StopIgnoringEvents(true);
            GameEvents.Contract.onContractsListChanged.Fire();
            System.StopIgnoringEvents();
        }

        /// <summary>
        /// Convert a byte array to a ConfigNode and then to a Strategy.
        /// If anything goes wrong it will return null.
        /// </summary>
        private static Strategy ConvertByteArrayToStrategy(byte[] data, int numBytes)
        {
            ConfigNode node;
            try
            {
                node = ConfigNodeSerializer.Deserialize(data, numBytes);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing strategy configNode: {e}");
                return null;
            }

            if (node == null)
            {
                LunaLog.LogError("[LMP]: Error, the strategy configNode was null.");
                return null;
            }

            Strategy strategy;
            try
            {
                strategy = new Strategy();
                strategy.Load(node);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error while deserializing strategy: {e}");
                return null;
            }

            return strategy;
        }
    }
}
