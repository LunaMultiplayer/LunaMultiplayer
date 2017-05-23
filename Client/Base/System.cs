using System.Collections.Generic;
using System.Linq;
using System.Text;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using Debug = UnityEngine.Debug;

namespace LunaClient.Base
{
    /// <summary>
    /// System base class. This class is made for a grouping logic.
    /// If you create a new system remember to call it on the SystemsHandler class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class System<T> : SystemBase, ISystem
        where T : class, ISystem, new()
    {
        #region Constructors
        /// <summary>
        /// Static constructor that creates the needed singleton
        /// </summary>
        static System()
        {
            Singleton = new T();
        }
        #endregion

        #region Field & Properties

        private Dictionary<string, RoutineDefinition> UpdateRoutines { get; } = new Dictionary<string, RoutineDefinition>();
        private Dictionary<string, RoutineDefinition> FixedUpdateRoutines { get; } = new Dictionary<string, RoutineDefinition>();

        #endregion

        /// <summary>
        /// Setups a routine that will be executed
        /// </summary>
        /// <param name="routine"></param>
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
        /// Removes a routine from the pool
        /// </summary>
        protected void RemoveRoutine(string routineName)
        {
            if (UpdateRoutines.ContainsKey(routineName))
            {
                UpdateRoutines.Remove(routineName);
            }
            else if (FixedUpdateRoutines.ContainsKey(routineName))
            {
                FixedUpdateRoutines.Remove(routineName);
            }
            else
            {
                Debug.LogError($"[LMP]: Routine {routineName} not found");
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

        /// <summary>
        /// System singleton
        /// </summary>
        public static T Singleton { get; set; }

        private bool _enabled;
        public virtual bool Enabled
        {
            get { return _enabled; }
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
                    DisableRoutines();
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
        /// Override to write code to execute when system is enabled
        /// </summary>
        public virtual void OnEnabled()
        {
            //Implement your own code
        }

        /// <summary>
        /// Override to write code to execute when system is disabled
        /// </summary>
        public virtual void OnDisabled()
        {
            //Implement your own code
        }

        /// <summary>
        /// When the system is disabled this method is called and it removes all the defined routines
        /// </summary>
        public virtual void DisableRoutines()
        {
            UpdateRoutines.Clear();
            FixedUpdateRoutines.Clear();
        }

        /// <summary>
        /// Update method. It will call the subscribed routines
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
                builder.AppendLine("Times in ms (average/min/max/now) ");

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