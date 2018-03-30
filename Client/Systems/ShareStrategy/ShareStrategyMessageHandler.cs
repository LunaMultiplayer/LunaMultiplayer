using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.ShareFunds;
using LunaClient.Systems.ShareReputation;
using LunaClient.Systems.ShareScience;
using LunaClient.Utilities;
using LunaCommon.Message.Data.ShareProgress;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Strategies;
using System;
using System.Collections.Concurrent;

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
                LunaLog.Log($"Queue StrategyUpdate with: {strategy.Name}");
                System.QueueAction(() =>
                {
                    StrategyUpdate(strategy);
                });
            }
        }

        private static void StrategyUpdate(StrategyInfo strategyInfo)
        {
            var incomingStrategyNode = ConvertByteArrayToConfigNode(strategyInfo.Data, strategyInfo.NumBytes);
            if (incomingStrategyNode == null) return;
            var incomingStrategyFactor = float.Parse(incomingStrategyNode.GetValue("factor"));
            var incomingStrategyIsActive = bool.Parse(incomingStrategyNode.GetValue("isActive"));

            //Don't listen to these events for the time this message is processing.
            System.StartIgnoringEvents();
            ShareFundsSystem.Singleton.StartIgnoringEvents();
            ShareScienceSystem.Singleton.StartIgnoringEvents();
            ShareReputationSystem.Singleton.StartIgnoringEvents();

            var strategyIndex = StrategySystem.Instance.Strategies.FindIndex(s => s.Config.Name == strategyInfo.Name);
            if (strategyIndex != -1)
            {
                if (incomingStrategyIsActive)
                {
                    StrategySystem.Instance.Strategies[strategyIndex].Factor = incomingStrategyFactor;
                    StrategySystem.Instance.Strategies[strategyIndex].Activate();   //could somehow throw an exception if the player was not yet in the strategy building.
                    LunaLog.Log($"StrategyUpdate received - strategy activated: {strategyInfo.Name}  - with factor: {incomingStrategyFactor}");
                }
                else
                {
                    StrategySystem.Instance.Strategies[strategyIndex].Factor = incomingStrategyFactor;
                    StrategySystem.Instance.Strategies[strategyIndex].Deactivate(); //could somehow throw an exception if the player was not yet in the strategy building.
                    LunaLog.Log($"StrategyUpdate received - strategy deactivated: {strategyInfo.Name}  - with factor: {incomingStrategyFactor}");
                }
            }

            Administration.Instance?.RedrawPanels();

            //Listen to the events again. Restore funds, science and reputation in case the contract action changed some of that.
            ShareFundsSystem.Singleton.StopIgnoringEvents(true);
            ShareScienceSystem.Singleton.StopIgnoringEvents(true);
            ShareReputationSystem.Singleton.StopIgnoringEvents(true);
            GameEvents.Contract.onContractsListChanged.Fire();
            System.StopIgnoringEvents();
        }

        /// <summary>
        /// Convert a byte array to a ConfigNode.
        /// If anything goes wrong it will return null.
        /// </summary>
        private static ConfigNode ConvertByteArrayToConfigNode(byte[] data, int numBytes)
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

            if (!node.HasValue("isActive"))
            {
                LunaLog.LogError("[LMP]: Error, the strategy configNode is invalid (isActive missing).");
                return null;
            }

            return node;
        }
    }
}
