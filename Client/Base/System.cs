using System.Diagnostics;
using LunaClient.Base.Interface;
using LunaClient.Utilities;

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
        /// <summary>
        /// Static constructor that creates the needed singleton
        /// </summary>
        static System()
        {
            Singleton = new T();
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
        /// Update wrapper
        /// </summary>
        public void RunUpdate()
        {
            var startClock = ProfilerData.LmpReferenceTime.ElapsedTicks;
            Update();
            UpdateProfiler.ReportTime(startClock);
        }

        /// <summary>
        /// Fixed update wrapper
        /// </summary>
        public void RunFixedUpdate()
        {
            var startClock = ProfilerData.LmpReferenceTime.ElapsedTicks;
            FixedUpdate();
            FixedUpdateProfiler.ReportTime(startClock);
        }
        
        /// <summary>
        /// Override to call your custom update functionallity
        /// </summary>
        public virtual void Update()
        {
            //Implement your own code
        }

        /// <summary>
        /// Override to call your custom FixedUpdate functionallity
        /// </summary>
        public virtual void FixedUpdate()
        {
            //Implement your own code
        }
    }
}