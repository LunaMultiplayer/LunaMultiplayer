using LunaClient.Base.Interface;
using LunaClient.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LunaClient.Base
{
    /// <summary>
    /// System base class. This class is made for a grouping logic.
    /// If you create a new system remember to call it on the SystemsHandler class
    /// </summary>
    public abstract class System : SystemBase, ISystem
    {
        #region Field & Properties

        /// <summary>
        /// This dictionary hold all the routines that will be executed during the Update()
        /// The key is the method name that will be executed
        /// </summary>
        private Dictionary<string, RoutineDefinition> UpdateRoutines { get; } = new Dictionary<string, RoutineDefinition>();

        /// <summary>
        /// This dictionary hold all the routines that will be executed during the FixedUpdate()
        /// The key is the method name that will be executed
        /// </summary>
        private Dictionary<string, RoutineDefinition> FixedUpdateRoutines { get; } = new Dictionary<string, RoutineDefinition>();

        #endregion

        /// <summary>
        /// Setups a routine that will be executed. You should normally call this method from the OnEnabled
        /// </summary>
        protected void SetupRoutine(RoutineDefinition routine)
        {
            if (routine.Execution == RoutineExecution.Update && !UpdateRoutines.ContainsKey(routine.Name))
            {
                UpdateRoutines.Add(routine.Name, routine);
            }
            else if (routine.Execution == RoutineExecution.FixedUpdate && !FixedUpdateRoutines.ContainsKey(routine.Name))
            {
                FixedUpdateRoutines.Add(routine.Name, routine);
            }
            else
            {
                Debug.LogError($"[LMP]: Routine {routine.Name} already defined");
            }
        }

        /// <summary>
        /// Changes the routine execution interval on the fly
        /// </summary>
        protected void ChangeRoutineExecutionInterval(string routineName, int newIntervalInMs)
        {
            if (UpdateRoutines.ContainsKey(routineName))
            {
                UpdateRoutines[routineName].IntervalInMs = newIntervalInMs;
            }
            else if (FixedUpdateRoutines.ContainsKey(routineName))
            {
                FixedUpdateRoutines[routineName].IntervalInMs = newIntervalInMs;
            }
            else
            {
                Debug.LogError($"[LMP]: Routine {routineName} not defined");
            }
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
                    RemoveRoutines();
                    OnDisabled();
                }
            }
        }

        /// <summary>
        /// Used to control the performance of the system
        /// </summary>
        private ProfilerData UpdateProfiler { get; } = new ProfilerData();

        /// <summary>
        /// Used to control the performance of the system
        /// </summary>
        private ProfilerData FixedUpdateProfiler { get; } = new ProfilerData();

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
        }

        /// <summary>
        /// Update method. It will call the subscribed routines. 
        /// Should be called only by the SystemsHandler class
        /// </summary>
        public void Update()
        {
            var startClock = ProfilerData.LmpReferenceTime.ElapsedTicks;

            foreach (var routine in UpdateRoutines)
            {
                routine.Value.RunRoutine();
            }

            UpdateProfiler.ReportTime(startClock);
        }

        /// <summary>
        /// Fixed update method. It will call the subscribed routines        
        /// Should be called only by the SystemsHandler class
        /// </summary>
        public void FixedUpdate()
        {
            var startClock = ProfilerData.LmpReferenceTime.ElapsedTicks;

            foreach (var routine in FixedUpdateRoutines)
            {
                routine.Value.RunRoutine();
            }

            FixedUpdateProfiler.ReportTime(startClock);
        }

        /// <summary>
        /// Get the performance counter of the system and it's routines
        /// </summary>
        public string GetProfilersData()
        {
            var builder = new StringBuilder();

            if (UpdateRoutines.Any() || FixedUpdateRoutines.Any())
                builder.AppendLine("Times in ms (average/max/min/now) ");

            if (UpdateRoutines.Any())
            {
                builder.Append("Total upd: ").Append(UpdateProfiler).AppendLine();
                foreach (var routine in UpdateRoutines)
                {
                    builder.Append(routine.Key).Append(": ").Append(routine.Value.Profiler).AppendLine();
                }
            }

            if (FixedUpdateRoutines.Any())
            {
                builder.Append("Total Fix upd: ").Append(FixedUpdateProfiler).AppendLine();
                foreach (var routine in FixedUpdateRoutines)
                {
                    builder.Append(routine.Key).Append(": ").Append(routine.Value.Profiler).AppendLine();
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Reset the performance counter of the system and it's routines
        /// </summary>
        public void ResetProfilers()
        {
            UpdateProfiler.Reset();
            FixedUpdateProfiler.Reset();

            foreach (var routine in UpdateRoutines)
            {
                routine.Value.Profiler.Reset();
            }

            foreach (var routine in FixedUpdateRoutines)
            {
                routine.Value.Profiler.Reset();
            }
        }
    }
}