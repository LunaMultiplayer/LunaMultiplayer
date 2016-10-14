using LunaClient.Base.Interface;

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

        public virtual bool Enabled { get; set; }

        /// <summary>
        /// Override to call your custom reset functionallity
        /// </summary>
        public virtual void Reset()
        {
            DoReset();
            Recreate();
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
        
        public virtual void LateUpdate()
        {
            //Implement your own code
        }

        /// <summary>
        /// Override to call your custom destroy functionallity
        /// </summary>
        protected virtual void DoReset()
        {
            Singleton.Enabled = false;
        }

        /// <summary>
        /// Override to call your custom recreation functionallity so the singleton is recreated
        /// </summary>
        protected virtual void Recreate()
        {
            Singleton = new T();
        }
    }
}