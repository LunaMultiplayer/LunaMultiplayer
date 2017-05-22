using System;
using System.Collections.Generic;
using System.Diagnostics;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using UnityEngine;
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

        private Dictionary<String, System.Diagnostics.Stopwatch> timerMap { get; } = new Dictionary<String, System.Diagnostics.Stopwatch>();
        private Dictionary<String, int> timerDuration { get; } = new Dictionary<String, int>();
        #endregion

        #region Timer Methods
        /// <summary>
        /// Sets up a new timer in the system with the given name
        /// </summary>
        /// <param name="timerName">Name of the timer</param>
        /// <param name="newDuration">New Duration in milliseconds</param>
        protected void setupTimer(String timerName, int newDurationMs)
        {
            if (!timerMap.ContainsKey(timerName))
            {
                timerMap[timerName] = System.Diagnostics.Stopwatch.StartNew();
            }
            timerDuration[timerName] = newDurationMs;
        }

        /// <summary>
        /// Returns whether it is time to send another update for this system.  
        /// If this method returns true, the internal timer is reset and restarted for the next transmission event.
        /// </summary>
        protected bool IsTimeForNextSend(String timerName)
        {
            if (!timerMap.ContainsKey(timerName))
            {
                throw new Exception("Timer requested but never defined with name:" + timerName);
            }

            System.Diagnostics.Stopwatch SendTimer = timerMap[timerName];
            int SendTimeIntervalMs = timerDuration[timerName];
            if (SendTimer.ElapsedMilliseconds > SendTimeIntervalMs)
            {
                SendTimer.Reset();
                SendTimer.Start();
                return true;
            }
            return false;
        }
        #endregion

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
                    OnDisabled();
                }
            }
        }

        /// <summary>
        /// Used to control the performance of the system
        /// </summary>
        public ProfilerData UpdateProfiler { get; } = new ProfilerData();

        /// <summary>
        /// Used to control the performance of the system
        /// </summary>
        public ProfilerData FixedUpdateProfiler { get; } = new ProfilerData();

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
        public void RunFixedUpdate()
        {
            var startClock = ProfilerData.LmpReferenceTime.ElapsedTicks;

            foreach (var routine in FixedUpdateRoutines)
            {
                routine.Value.RunRoutine();
            }

            FixedUpdateProfiler.ReportTime(startClock);
        }
    }
}