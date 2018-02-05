using LunaClient.Base.Interface;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable ForCanBeConvertedToForeach

namespace LunaClient.Base
{
    /// <summary>
    /// System base class. This class is made for a grouping logic.
    /// If you create a new system remember to call it on the SystemsHandler class
    /// </summary>
    public abstract class System : SystemBase, ISystem
    {
        #region Field & Properties

        public abstract string SystemName { get; }

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
        protected void ChangeRoutineExecutionInterval(string routineName, int newIntervalInMs)
        {
            var routine = FixedUpdateRoutines.FirstOrDefault(r => r.Name == routineName);
            if (routine != null)
            {
                routine.IntervalInMs = newIntervalInMs;
                return;
            }

            routine = UpdateRoutines.FirstOrDefault(r => r.Name == routineName);
            if (routine != null)
            {
                routine.IntervalInMs = newIntervalInMs;
                return;
            }

            routine = LateUpdateRoutines.FirstOrDefault(r => r.Name == routineName);
            if (routine != null)
            {
                routine.IntervalInMs = newIntervalInMs;
                return;
            }

            LunaLog.LogError($"[LMP]: Routine {routineName} not defined");
        }

        private bool _enabled;

        /// <summary>
        /// Enables or disables the system. When disabling, all the defined routines will be removed
        /// </summary>
        public virtual bool Enabled
        {
            get => _enabled;
            set
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