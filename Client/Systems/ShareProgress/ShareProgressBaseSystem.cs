using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base;
using LunaClient.Base.Interface;

namespace LunaClient.Systems.ShareProgress
{
    public abstract class ShareProgressBaseSystem<T, TS, TH> : MessageSystem<T, TS, TH> 
        where T : System<T>, new() 
        where TS : class, IMessageSender, new()
        where TH : class, IMessageHandler, new()
    {
        public bool IgnoreEvents { get; protected set; }

        private Queue<Action> _actionQueue;
        public int ActionQueueCount => _actionQueue?.Count ?? 0;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            IgnoreEvents = false;
            _actionQueue = new Queue<Action>();

            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RunQueue));
        }

        /// <summary>
        /// Returns true if the action queue can run.
        /// For example check if some variable is null like Funding.Instance.
        /// </summary>
        /// <returns></returns>
        protected abstract bool ActionDependencyReady();

        /// <summary>
        /// Saves the current state of the variables that this system is tracking.
        /// </summary>
        public abstract void SaveState();

        /// <summary>
        /// Restores the state of variables this system is tracking from the last save.
        /// </summary>
        public abstract void RestoreState();

        /// <summary>
        /// Start ignoring the incoming ksp events.
        /// Only call this if you are sure ActionDependenyReady() returns true.
        /// For example in an QueueAction().
        /// </summary>
        public void StartIgnoringEvents()
        {
            SaveState();
            IgnoreEvents = true;
        }

        /// <summary>
        /// Stop ignoring the incoming ksp events.
        /// Only call this if you are sure ActionDependenyReady() returns true.
        /// For example in an QueueAction().
        /// </summary>
        public void StopIgnoringEvents(bool restoreOldValue = false)
        {
            if (restoreOldValue)
                RestoreState();

            IgnoreEvents = false;
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
            while (_actionQueue.Count > 0 && ActionDependencyReady())
            {
                var action = _actionQueue.Dequeue();
                action?.Invoke();
            }
        }
    }
}
