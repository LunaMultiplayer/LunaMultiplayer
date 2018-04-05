using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contracts;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.ShareAchievements;
using LunaClient.Systems.ShareContracts;
using LunaClient.Systems.ShareProgress;
using LunaClient.Systems.ShareReputation;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Systems.ShareCareer
{
    /// <summary>
    /// Class for holding a queue of career actions that will be processed in the right order.
    /// The Systems ShareContracts, ShareAchievements and ShareReputation will use this queue instead of their own
    /// to keep the right order of execution.
    /// </summary>
    public class ShareCareerSystem : Base.System<ShareCareerSystem>
    {
        public override string SystemName { get; } = nameof(ShareCareerSystem);

        private Queue<Action> _actionQueue;
        public int ActionQueueCount => _actionQueue?.Count ?? 0;

        //Dependencies to run the queue
        protected bool ShareSystemReady => ContractSystem.Instance != null && ContractSystem.Instance.Contracts.Count != 0 &&
                                           Funding.Instance != null && ResearchAndDevelopment.Instance != null &&
                                           Reputation.Instance != null && ProgressTracking.Instance != null &&
                                           Time.timeSinceLevelLoad > 1f;

        protected override void OnEnabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnEnabled();

            _actionQueue = new Queue<Action>();

            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RunQueue));
        }

        protected override void OnDisabled()
        {
            if (SettingsSystem.ServerSettings.GameMode != GameMode.Career) return;

            base.OnDisabled();
        }

        /// <summary>
        /// Queue an action that is dependent on the ActionDependency and will run
        /// if the ActionDependencyReady method returns true. For example a action like:
        /// Funding.Instance.SetFunds(1000, TransactionReasons.None);
        /// </summary>
        /// <param name="action"></param>
        public void QueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
            RunQueue();
        }

        /// <summary>
        /// Run the queue and call the actions.
        /// </summary>
        private void RunQueue()
        {
            while (_actionQueue.Count > 0 && ShareSystemReady)
            {
                var action = _actionQueue.Dequeue();
                action?.Invoke();
            }
        }

    }
}
