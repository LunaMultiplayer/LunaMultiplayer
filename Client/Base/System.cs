using LunaClient.Base.Interface;
using LunaClient.Events;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable ForCanBeConvertedToForeach

namespace LunaClient.Base
{
    /// <summary>
    /// System base class. This class is made for a grouping logic.
    /// </summary>
    public abstract class System<T> : SystemBase, ISystem
        where T : ISystem, new()
    {
        #region Field & Properties

        public static T Singleton { get; } = new T();

        /// <summary>
        /// Put here the system name
        /// </summary>
        public abstract string SystemName { get; }

        /// <summary>
        /// When systems are loaded they are ordered by this number. Put a low value if you want to run your system before the others
        /// </summary>
        public virtual int ExecutionOrder { get; } = 0;

        /// <summary>
        /// This list hold all the routines that will be executed during the FixedUpdate()
        /// The key is the method name that will be executed
        /// </summary>
        private List<RoutineDefinition> FixedUpdateRoutines { get; } = new List<RoutineDefinition>();

        /// <summary>
        /// This list hold all the routines that will be executed during the Update()
        /// The key is the method name that will be executed
        /// </summary>
        private List<RoutineDefinition> UpdateRoutines { get; } = new List<RoutineDefinition>();

        /// <summary>
        /// This list hold all the routines that will be executed during the LateUpdate()
        /// The key is the method name that will be executed
        /// </summary>
        private List<RoutineDefinition> LateUpdateRoutines { get; } = new List<RoutineDefinition>();

        #endregion

        /// <summary>
        /// We subscribe for the network event changes on the constructor
        /// </summary>
        protected System() => NetworkEvent.onNetworkStatusChanged.Add(NetworkEventHandler);

        /// <summary>
        /// Handle here what happens to your system when the network status changes. By default it will be disabled if we disconnect and enable just before starting
        /// </summary>
        protected virtual void NetworkEventHandler(ClientState data)
        {
            if (data <= ClientState.Disconnected)
            {
                Enabled = false;
            }

            if (data == ClientState.Running)
            {
                Enabled = true;
            }
        }

        /// <summary>
        /// Return true if your system needs to be always enabled
        /// </summary>
        protected virtual bool AlwaysEnabled { get; } = false;

        /// <summary>
        /// Setups a routine that will be executed. You should normally call this method from the OnEnabled
        /// </summary>
        protected void SetupRoutine(RoutineDefinition routine)
        {
            if (routine == null)
            {
                LunaLog.LogError($"[LMP]: Cannot set a null routine!");
                return;
            }

            if (routine.Execution == RoutineExecution.FixedUpdate && !FixedUpdateRoutines.Any(r => r.Name == routine.Name))
            {
                FixedUpdateRoutines.Add(routine);
            }
            else if (routine.Execution == RoutineExecution.Update && !UpdateRoutines.Any(r => r.Name == routine.Name))
            {
                UpdateRoutines.Add(routine);
            }
            else if (routine.Execution == RoutineExecution.LateUpdate && !LateUpdateRoutines.Any(r => r.Name == routine.Name))
            {
                LateUpdateRoutines.Add(routine);
            }
            else
            {
                LunaLog.LogError($"[LMP]: Routine {routine.Name} already defined");
            }
        }

        /// <summary>
        /// Changes the routine execution interval on the fly
        /// </summary>
        protected void ChangeRoutineExecutionInterval(RoutineExecution execution, string routineName, int newIntervalInMs)
        {
            RoutineDefinition routine;
            switch (execution)
            {
                case RoutineExecution.FixedUpdate:
                    routine = FixedUpdateRoutines.FirstOrDefault(r => r.Name == routineName);
                    break;
                case RoutineExecution.Update:
                    routine = UpdateRoutines.FirstOrDefault(r => r.Name == routineName);
                    break;
                case RoutineExecution.LateUpdate:
                    routine = LateUpdateRoutines.FirstOrDefault(r => r.Name == routineName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(execution), execution, null);
            }

            if (routine != null)
            {
                routine.IntervalInMs = newIntervalInMs;
            }
            else
            {
                LunaLog.LogError($"[LMP]: Routine {execution}/{routineName} not defined");
            }

        }

        private bool _enabled;

        /// <summary>
        /// Enables or disables the system. When disabling, all the defined routines will be removed
        /// </summary>
        public virtual bool Enabled
        {
            get => AlwaysEnabled || _enabled;
            set
            {
                if (!AlwaysEnabled)
                {
                    if (!_enabled && value)
                    {
                        _enabled = true;
                        OnEnabled();
                    }
                    else if (_enabled && !value)
                    {
                        _enabled = false;
                        OnDisabled();
                        RemoveRoutines();
                    }
                }
            }
        }

        /// <summary>
        /// Override to write code to execute when system is enabled.
        /// Use this method to SetupRoutines or to subscribe to game events
        /// </summary>
        protected virtual void OnEnabled()
        {
            //Implement your own code
        }

        /// <summary>
        /// Override to write code to execute when system is disabled.
        /// Use this method to unsubscribe from game events
        /// After this method is run, RemoveRoutines() will be called
        /// </summary>
        protected virtual void OnDisabled()
        {
            //Implement your own code
        }

        /// <summary>
        /// When the system is disabled this method is called and it removes all the defined routines.
        /// Override it if you want to avoid it.
        /// </summary>
        protected virtual void RemoveRoutines()
        {
            UpdateRoutines.Clear();
            FixedUpdateRoutines.Clear();
            LateUpdateRoutines.Clear();
        }


        /// <summary>
        /// Fixed update method. It will call the subscribed routines        
        /// Should be called only by the SystemsHandler class
        /// </summary>
        public void FixedUpdate()
        {
            for (var i = 0; i < FixedUpdateRoutines.Count; i++)
            {
                FixedUpdateRoutines[i]?.RunRoutine();
            }
        }

        /// <summary>
        /// Update method. It will call the subscribed routines. 
        /// Should be called only by the SystemsHandler class
        /// </summary>
        public void Update()
        {
            for (var i = 0; i < UpdateRoutines.Count; i++)
            {
                UpdateRoutines[i]?.RunRoutine();
            }
        }

        /// <summary>
        /// LateUpdate method. It will call the subscribed routines. 
        /// Should be called only by the SystemsHandler class
        /// </summary>
        public void LateUpdate()
        {
            for (var i = 0; i < LateUpdateRoutines.Count; i++)
            {
                LateUpdateRoutines[i]?.RunRoutine();
            }
        }
    }
}
